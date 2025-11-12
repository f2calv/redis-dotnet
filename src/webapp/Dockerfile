FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
#COPY ["Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["src/webapp/webapp.csproj", "src/webapp/"]
COPY ["src/SharedLibrary/SharedLibrary.csproj", "src/SharedLibrary/"]
RUN dotnet restore "src/webapp/webapp.csproj"
COPY . .

RUN dotnet publish "src/webapp/webapp.csproj" -c Release -o /app/publish

#FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final #missing tzdata?
#FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "webapp.dll"]
