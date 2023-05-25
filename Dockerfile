FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DiscordChannelsBot.csproj", "./"]
RUN dotnet restore "DiscordChannelsBot.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "DiscordChannelsBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiscordChannelsBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiscordChannelsBot.dll"]
