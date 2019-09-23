
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
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