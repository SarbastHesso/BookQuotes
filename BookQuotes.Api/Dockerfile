# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BookQuotes.Api/BookQuotes.Api.csproj", "BookQuotes.Api/"]
RUN dotnet restore "BookQuotes.Api/BookQuotes.Api.csproj"
COPY . .
WORKDIR /src/BookQuotes.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "BookQuotes.Api.dll"]
