FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/Common/Common.csproj src/Common/
COPY src/Loans/Loans.csproj src/Loans/
RUN dotnet restore src/Loans/Loans.csproj
COPY src/Common/ src/Common/
COPY src/Loans/ src/Loans/
RUN dotnet publish src/Loans/Loans.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Loans.dll"]
