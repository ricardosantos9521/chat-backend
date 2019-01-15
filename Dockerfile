
FROM microsoft/dotnet:2.1.1-aspnetcore-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["src/SignalRServer.csproj", "."]
RUN dotnet restore "SignalRServer.csproj"
COPY /src .
RUN dotnet build "SignalRServer.csproj" -c Release -o /app

FROM build AS publish
ARG buildnumber="notset"
RUN dotnet publish "SignalRServer.csproj" -c Release --version-suffix $buildnumber -o /app

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS "http://+"
EXPOSE 80
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SignalRServer.dll"]