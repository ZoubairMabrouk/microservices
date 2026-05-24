FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./MdxServices.csproj ./
RUN dotnet restore "./MdxServices.csproj"

COPY . ./
RUN dotnet publish "./MdxServices.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8088
EXPOSE 8088

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "MdxServices.dll"]
