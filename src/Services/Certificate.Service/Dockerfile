FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
RUN mkdir -p /app/certificates
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Certificate.Service.dll"]
