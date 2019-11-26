## Base Setup
FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS base
WORKDIR /app

## Build
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build

WORKDIR /src
COPY . .

RUN dotnet publish -c Release -o /app/out

## Runtime
FROM base
COPY --from=build /app/out .
ENTRYPOINT [ "dotnet", "CampusNetCheckerService.dll" ]

