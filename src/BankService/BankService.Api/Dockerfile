#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["src/BankService/BankService.Api/BankService.Api.csproj", "src/BankService/BankService.Api/"]
COPY ["src/BuildingBlocks/Healthchecks/Healthchecks.csproj", "src/BuildingBlocks/Healthchecks/"]
RUN dotnet restore "src/BankService/BankService.Api/BankService.Api.csproj"
COPY . .
WORKDIR "/src/src/BankService/BankService.Api"
RUN dotnet build "BankService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bankr.BankService.Api.dll"]
