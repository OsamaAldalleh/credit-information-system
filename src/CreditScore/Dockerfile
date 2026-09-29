FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/Common/Common.csproj src/Common/
COPY src/CreditScore/CreditScore.csproj src/CreditScore/
RUN dotnet restore src/CreditScore/CreditScore.csproj
COPY src/Common/ src/Common/
COPY src/CreditScore/ src/CreditScore/
RUN dotnet publish src/CreditScore/CreditScore.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CreditScore.dll"]
