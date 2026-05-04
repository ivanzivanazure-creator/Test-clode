FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["AccountingERP.API.csproj", "."]
COPY ["../AccountingERP.Infrastructure/AccountingERP.Infrastructure.csproj", "../AccountingERP.Infrastructure/"]
COPY ["../AccountingERP.Application/AccountingERP.Application.csproj", "../AccountingERP.Application/"]
COPY ["../AccountingERP.Domain/AccountingERP.Domain.csproj", "../AccountingERP.Domain/"]
RUN dotnet restore
COPY . .
WORKDIR "/src"
RUN dotnet build "AccountingERP.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AccountingERP.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccountingERP.API.dll"]
