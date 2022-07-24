FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["samples/TestDotNetSite/TestDotNetSite.csproj", "TestDotNetSite/"]
RUN dotnet restore "samples/TestDotNetSite/TestDotNetSite.csproj"
COPY . .
WORKDIR "/src/TestDotNetSite"
RUN dotnet build "TestDotNetSite.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestDotNetSite.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestDotNetSite.dll"]
