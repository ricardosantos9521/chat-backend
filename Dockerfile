
FROM microsoft/dotnet:2.2.0-aspnetcore-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["src/SignalRServer.csproj", "."]
RUN dotnet restore "SignalRServer.csproj"
COPY /src .
RUN dotnet build "SignalRServer.csproj" -c Debug -o /app

FROM build AS publish
ARG buildnumber="notset"
RUN dotnet publish "SignalRServer.csproj" -c Debug --version-suffix $buildnumber -o /app

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT Development
ENV ASPNETCORE_URLS "http://+"
EXPOSE 80
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SignalRServer.dll"]