FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/webapp/webapp.csproj", "src/webapp/"]
COPY ["src/SharedLibrary/SharedLibrary.csproj", "src/SharedLibrary/"]
RUN dotnet restore "src/webapp/webapp.csproj"
COPY . .

RUN dotnet publish "src/webapp/webapp.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "webapp.dll"]