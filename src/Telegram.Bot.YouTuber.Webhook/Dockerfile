FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Telegram.Bot.YouTuber.Webhook/Telegram.Bot.YouTuber.Webhook.csproj", "src/Telegram.Bot.YouTuber.Webhook/"]
RUN dotnet restore "src/Telegram.Bot.YouTuber.Webhook/Telegram.Bot.YouTuber.Webhook.csproj"
COPY . .
WORKDIR "/src/src/Telegram.Bot.YouTuber.Webhook"
RUN dotnet build "Telegram.Bot.YouTuber.Webhook.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Telegram.Bot.YouTuber.Webhook.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Telegram.Bot.YouTuber.Webhook.dll"]
