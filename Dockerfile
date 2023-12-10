FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

COPY PassGuardia.Contracts/PassGuardia.Contracts.csproj ./PassGuardia.Contracts/
COPY PassGuardia.Domain/PassGuardia.Domain.csproj ./PassGuardia.Domain/
COPY PassGuardia.Api/PassGuardia.Api.csproj ./PassGuardia.Api/
RUN dotnet restore PassGuardia.Api

COPY . .
RUN dotnet build --no-restore -c Release PassGuardia.Api
RUN dotnet publish --no-restore --no-build -c Release -o out PassGuardia.Api

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "PassGuardia.Api.dll"]
