FROM mcr.microsoft.com/dotnet/sdk:9.0.304 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and publish app
COPY . ./
RUN dotnet publish -c Release -o /app/publish --runtime linux-x64 --self-contained false /p:PublishTrimmed=false

# Use SDK image again for runtime (has runtime + SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0.304
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GpsApp.dll"]

