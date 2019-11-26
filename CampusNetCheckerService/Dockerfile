## Build
FROM microsoft/dotnet:sdk AS build-env

WORKDIR /src
COPY . .

RUN dotnet publish -c Release -o /app/out

## Runtime
FROM microsoft/dotnet:aspnetcore-runtime

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "CampusNetCheckerService.dll" ]
