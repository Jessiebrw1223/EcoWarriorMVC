FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EcoWarriorMvc.csproj", "./"]
RUN dotnet restore "EcoWarriorMvc.csproj"
COPY . .
RUN dotnet publish "EcoWarriorMvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcoWarriorMvc.dll"]
