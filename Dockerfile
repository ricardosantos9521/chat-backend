
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["src/Chat.Backend.csproj", "."]
RUN dotnet restore "Chat.Backend.csproj"
COPY /src .
RUN dotnet build "Chat.Backend.csproj" -c Release -o /app

FROM build AS publish
ARG buildnumber="notset"
RUN dotnet publish "Chat.Backend.csproj" -c Release --version-suffix $buildnumber -o /app

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS "http://+"
EXPOSE 80
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Chat.Backend.dll"]