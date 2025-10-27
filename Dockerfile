# ------------------------------
# Stage 1: Build & Publish
# ------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY FilestoreMicroS/*.csproj ./FilestoreMicroS/
RUN dotnet restore FilestoreMicroS/FilestoreMicroS.csproj

# Копируем все исходники проекта
COPY FilestoreMicroS/ ./FilestoreMicroS/

# Публикуем проект в папку /app/publish
RUN dotnet publish FilestoreMicroS/FilestoreMicroS.csproj -c Release -o /app/publish /p:UseAppHost=false

# ------------------------------
# Stage 2: Runtime
# ------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Копируем собранные файлы из стадии сборки
COPY --from=build /app/publish .

# Указываем точку входа приложения
ENTRYPOINT ["dotnet", "FilestoreMicroS.dll"]
