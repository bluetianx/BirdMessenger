FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY ./ /publish
WORKDIR /publish
EXPOSE 5000
ENTRYPOINT ["dotnet","testDotNetSite.dll"]