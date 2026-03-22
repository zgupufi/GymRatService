# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GymRatService/GymRatService.csproj", "GymRatService/"]
RUN dotnet restore "GymRatService/GymRatService.csproj"
COPY . .
WORKDIR "/src/GymRatService"
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "GymRatService.dll"]
