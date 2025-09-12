# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0.304 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /out --runtime linux-x64 --self-contained false /p:PublishTrimmed=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/sdk:9.0.304
WORKDIR /app

COPY --from=build /out .

ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GpsApp.dll"]


