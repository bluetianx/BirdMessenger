FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder
COPY ./ /src
WORKDIR /src
RUN dotnet publish -c Release -o /publish 

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS Runtime
WORKDIR /Project 
COPY --from=builder /publish ./
EXPOSE 5000
ENTRYPOINT ["dotnet","testDotNetSite.dll"]