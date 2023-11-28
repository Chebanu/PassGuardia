FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

COPY PassGuardia.sln .
COPY PassGuardia.Contracts/PassGuardia.Contracts.csproj ./PassGuardia.Contracts/
COPY PassGuardia.Domain/PassGuardia.Domain.csproj ./PassGuardia.Domain/
COPY PassGuardia/PassGuardia.Api.csproj ./PassGuardia/
RUN dotnet restore

COPY . .
RUN dotnet build --no-restore -c Release
RUN dotnet publish --no-restore --no-build -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "PassGuardia.Api.dll"]
