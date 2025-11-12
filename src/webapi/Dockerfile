FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["src/webapi/webapi.csproj", "src/webapi/"]
COPY ["src/SharedLibrary/SharedLibrary.csproj", "src/SharedLibrary/"]
RUN dotnet restore "src/webapi/webapi.csproj"
COPY . .

RUN dotnet publish "src/webapi/webapi.csproj" -c Release -o /app/publish

#FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final #missing tzdata?
#FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "webapi.dll"]
