FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/Common/Common.csproj src/Common/
COPY src/Litigations/Litigations.csproj src/Litigations/
RUN dotnet restore src/Litigations/Litigations.csproj
COPY src/Common/ src/Common/
COPY src/Litigations/ src/Litigations/
RUN dotnet publish src/Litigations/Litigations.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Litigations.dll"]
