FROM mcr.microsoft.com/dotnet/aspnet:5.0.1-buster-slim
WORKDIR /app
EXPOSE 5001
EXPOSE 5000
COPY . .
ENTRYPOINT ["dotnet", "Antares.Service.History.dll"]
