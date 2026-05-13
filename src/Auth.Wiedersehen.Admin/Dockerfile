FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Auth.Wiedersehen.Admin.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish -c $BUILD_CONFIGURATION --self-contained --runtime linux-musl-x64 -o /app/publish

FROM alpine:latest AS final
RUN apk add --no-cache libgcc libstdc++ icu-libs
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./Auth.Wiedersehen.Admin"]
