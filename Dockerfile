FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY isgood/isgood.sln ./isgood/
COPY isgood/isgood.csproj ./isgood/
RUN dotnet restore ./isgood/

# copy everything else and build app
COPY isgood/. ./isgood/
WORKDIR /source/isgood
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app ./

EXPOSE 1883
EXPOSE 5000

ENTRYPOINT ["dotnet", "isgood.dll"]
