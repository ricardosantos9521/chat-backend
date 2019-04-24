
FROM microsoft/dotnet:2.2.0-aspnetcore-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["src/ChatTest.Server.csproj", "."]
RUN dotnet restore "ChatTest.Server.csproj"
COPY /src .
RUN dotnet build "ChatTest.Server.csproj" -c Release -o /app

FROM build AS publish
ARG buildnumber="notset"
RUN dotnet publish "ChatTest.Server.csproj" -c Release --version-suffix $buildnumber -o /app

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS "http://+"
EXPOSE 80
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ChatTest.Server.dll"]