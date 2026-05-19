using BookQuotes.Api.Data;
using BookQuotes.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using System;

namespace BookQuotes.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // If AZURE_KEY_VAULT_URI is set as an environment variable, load secrets from Key Vault
            var keyVaultUri = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI");
            if (!string.IsNullOrWhiteSpace(keyVaultUri))
            {
                try
                {
                    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
                }
                catch (Exception)
                {
                    // Do not block startup if Key Vault is unavailable during local development.
                }
            }

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

            // Configure EF Core provider based on configuration (Default: SqlServer)
            var dbProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
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
                    builder.Services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
                    break;
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
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
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


            builder.Services.AddCors(options =>
            {
                // Allow configuring allowed origins via configuration or env var `CORS_ALLOWED_ORIGINS` (comma-separated)
                var origins = builder.Configuration["Cors:AllowedOrigins"] ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
                string[] allowed = new string[] { "http://localhost:4200" };
                if (!string.IsNullOrWhiteSpace(origins))
                {
                    allowed = origins.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                }

                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins(allowed)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddScoped<TokenService>();

            // Configure DataProtection key persistence when DataProtection:KeyPath is provided.
            var dataProtectionPath = builder.Configuration["DataProtection:KeyPath"];
            if (!string.IsNullOrWhiteSpace(dataProtectionPath))
            {
                try
                {
                    builder.Services.AddDataProtection()
                        .PersistKeysToFileSystem(new System.IO.DirectoryInfo(dataProtectionPath));
                }
                catch (Exception ex)
                {
                    // If key path is invalid or not writable, continue but log the issue at runtime.
                    // Runtime logger will surface this during startup.
                }
            }

            var app = builder.Build();

            // Apply any pending migrations on startup (useful for Docker/dev workflows)
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                    if (db.Database.IsSqlite())
                    {
                        db.Database.EnsureCreated();
                    }
                    else
                    {
                        // Require explicit opt-in for applying migrations in non-development environments.
                        // Set environment variable `AUTO_APPLY_MIGRATIONS=true` in CI/CD or host only when you want automatic migration.
                        var autoApply = Environment.GetEnvironmentVariable("AUTO_APPLY_MIGRATIONS")?.ToLowerInvariant() == "true";
                        if (!app.Environment.IsDevelopment() && autoApply)
                        {
                            db.Database.Migrate();
                        }
                        else if (!app.Environment.IsDevelopment())
                        {
                            logger.LogWarning("Automatic migrations are disabled in non-development environment. Set AUTO_APPLY_MIGRATIONS=true to enable.");
                        }
                        else
                        {
                            logger.LogInformation("Skipping automatic EF Core migrations in Development environment.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                if (app.Environment.IsDevelopment())
                {
                    // In development/docker workflows we log and continue so the app stays reachable for debugging.
                    logger.LogError(ex, "An error occurred while migrating or initializing the database. Continuing without applying migrations (Development mode).");
                }
                else
                {
                    // In non-development environments (staging/production) fail fast so issues are visible.
                    logger.LogError(ex, "An error occurred while migrating or initializing the database.");
                    throw;
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAngular");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            // Simple health endpoint for smoke checks
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));


            app.Run();
        }
    }
}
