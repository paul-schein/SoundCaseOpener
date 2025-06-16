FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG FRAMEWORK=net9.0
WORKDIR /src
COPY ["SoundCaseOpener/SoundCaseOpener.csproj", "SoundCaseOpener/"]
COPY ["SoundCaseOpener.Core/SoundCaseOpener.Core.csproj", "SoundCaseOpener.Core/"]
COPY ["SoundCaseOpener.Persistence/SoundCaseOpener.Persistence.csproj", "SoundCaseOpener.Persistence/"]
COPY ["SoundCaseOpener.Shared/SoundCaseOpener.Shared.csproj", "SoundCaseOpener.Shared/"]
RUN dotnet restore "SoundCaseOpener/SoundCaseOpener.csproj" -p:TargetFramework=$FRAMEWORK
COPY . .
WORKDIR "/src/SoundCaseOpener"
RUN dotnet build "SoundCaseOpener.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SoundCaseOpener.csproj" -c $BUILD_CONFIGURATION -o /app/publish -r linux-x64 /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
ARG PORT=5200
WORKDIR /app
RUN mkdir Logs
EXPOSE $PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoundCaseOpener.dll"]