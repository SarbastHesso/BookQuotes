using BookQuotes.Api.Data;
using BookQuotes.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.RateLimiting;

namespace BookQuotes.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = false;
            });


            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var dataProtectionKeyPath = builder.Configuration["DataProtection:KeyPath"];
            var dataProtectionBuilder = builder.Services.AddDataProtection();
            if (!string.IsNullOrWhiteSpace(dataProtectionKeyPath))
            {
                dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyPath));
            }

            // Configure EF Core provider based on configuration (Default: SqlServer)
            var dbProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
            bool usingSqliteFallback = false;
            switch (dbProvider?.ToLowerInvariant())
            {
                case "sqlite":
                    builder.Services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=bookquotes.db"));
                    break;
                case "postgres":
                case "postgresql":
                    builder.Services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection") ?? "Host=localhost;Database=bookquotes;Username=postgres;Password=postgres"));
                    break;
                default:
                    var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrWhiteSpace(defaultConn))
                    {
                        // Fallback for CI/tests when no DefaultConnection is configured
                        builder.Services.AddDbContext<AppDbContext>(options =>
                            options.UseSqlite("Data Source=bookquotes_test.db"));
                        usingSqliteFallback = true;
                    }
                    else
                    {
                        builder.Services.AddDbContext<AppDbContext>(options =>
                            options.UseSqlServer(defaultConn));
                    }
                    break;
            }

            // Ensure we have a non-null signing key for tests/CI. Prefer configured Jwt:Key, then STAGING_JWT_KEY env/secret, fallback to a dev/test key.
            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                jwtKey = builder.Configuration["STAGING_JWT_KEY"] ?? System.Environment.GetEnvironmentVariable("STAGING_JWT_KEY") ?? "BookQuotes_Dev_Test_Key_ChangeMe";
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)
                        ),
                        ClockSkew = TimeSpan.Zero
                    };

                    // Custom 401 message
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();

                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";

                            var result = JsonSerializer.Serialize(new
                            {
                                error = "Authentication token is missing or invalid."
                            });

                            return context.Response.WriteAsync(result);
                        }
                    };
                });

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter("auth", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 10;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });
            });

            var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? ["http://localhost:4200"];

            var allowedMethods = builder.Configuration["Cors:AllowedMethods"]
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? ["GET", "POST", "PUT", "DELETE", "OPTIONS"];

            var allowedHeaders = builder.Configuration["Cors:AllowedHeaders"]
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? ["Authorization", "Content-Type"];

            var allowCredentials = builder.Configuration.GetValue("Cors:AllowCredentials", true);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .WithHeaders(allowedHeaders)
                          .WithMethods(allowedMethods);

                    if (allowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                });
            });

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddScoped<TokenService>();

            var app = builder.Build();

            // Apply any pending migrations on startup (useful for local development only)
            if (app.Environment.IsDevelopment() || usingSqliteFallback)
            {
                try
                {
                    using (var scope = app.Services.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        if (usingSqliteFallback)
                        {
                            // EnsureCreated is acceptable for ephemeral CI/test DBs when migrations
                            // are not in sync; it creates schema from the current model.
                            db.Database.EnsureCreated();
                        }
                        else
                        {
                            db.Database.Migrate();
                        }
                    }
                }
                catch (Exception ex)
                {
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating or initializing the database.");

                    if (ex is InvalidOperationException && ex.Message?.Contains("PendingModelChangesWarning") == true)
                    {
                        logger.LogWarning("Pending EF Core model changes detected. Skipping automatic migration to avoid modifying the existing database.");
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseForwardedHeaders();

            app.UseHttpsRedirection();

            app.UseCors("AllowAngular");

            app.UseRateLimiter();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            // Simple health endpoint for smoke checks
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
            app.MapGet("/health/live", () => Results.Ok(new { status = "Live" }));


            app.Run();
        }
    }
}
