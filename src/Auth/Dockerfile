FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/Common/Common.csproj src/Common/
COPY src/Auth/Auth.csproj src/Auth/
RUN dotnet restore src/Auth/Auth.csproj
COPY src/Common/ src/Common/
COPY src/Auth/ src/Auth/
RUN dotnet publish src/Auth/Auth.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Auth.dll"]
