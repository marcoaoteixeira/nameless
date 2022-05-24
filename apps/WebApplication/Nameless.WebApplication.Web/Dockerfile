#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["apps/WebApplication/Nameless.WebApplication.Web/Nameless.WebApplication.Web.csproj", "apps/WebApplication/Nameless.WebApplication.Web/"]
RUN dotnet restore "apps/WebApplication/Nameless.WebApplication.Web/Nameless.WebApplication.Web.csproj"
COPY . .
WORKDIR "/src/apps/WebApplication/Nameless.WebApplication.Web"
RUN dotnet build "Nameless.WebApplication.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nameless.WebApplication.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nameless.WebApplication.Web.dll"]