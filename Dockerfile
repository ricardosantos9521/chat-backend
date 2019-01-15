
FROM microsoft/dotnet:2.2.0-aspnetcore-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["src/payLaterApi.csproj", "src/"]
RUN dotnet restore "src/payLaterApi.csproj"
COPY . .
WORKDIR /src/test
RUN dotnet test -c Release
WORKDIR /src/src
RUN dotnet build "payLaterApi.csproj" -c Release -o /app

FROM build AS publish
ARG buildnumber="0"
RUN dotnet publish "payLaterApi.csproj" -c Release --version-suffix $buildnumber -o /app

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS "http://+"
EXPOSE 80
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "payLaterApi.dll"]