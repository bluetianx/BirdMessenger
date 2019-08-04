FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
COPY ./ /publish
WORKDIR /publish
EXPOSE 5000
ENTRYPOINT ["dotnet","testDotNetSite.dll"]