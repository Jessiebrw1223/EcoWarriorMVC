# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EcoWarriorMVC.csproj", "./"]
RUN dotnet restore "./EcoWarriorMVC.csproj"

COPY . .
RUN dotnet publish "./EcoWarriorMVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet EcoWarriorMVC.dll"]
