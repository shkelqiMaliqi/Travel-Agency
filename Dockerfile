FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Travel Agency Portal/Travel Agency Portal/Travel Agency Portal.csproj", "Travel Agency Portal/Travel Agency Portal/"]
COPY ["Travel Agency Portal/Travel Agency Portal.Tests/Travel Agency Portal.Tests.csproj", "Travel Agency Portal/Travel Agency Portal.Tests/"]
RUN dotnet restore "Travel Agency Portal/Travel Agency Portal/Travel Agency Portal.csproj"

COPY . .
WORKDIR "/src/Travel Agency Portal/Travel Agency Portal"
RUN dotnet publish "Travel Agency Portal.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Travel Agency Portal.dll"]
