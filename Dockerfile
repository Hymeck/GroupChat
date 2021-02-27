FROM mcr.microsoft.com/dotnet/aspnet:5.0

COPY release app/
WORKDIR app/
# todo: don't forget to rename it if rename entry point project
ENTRYPOINT ["dotnet", "Entry.dll"]