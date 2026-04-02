# 1. Aşama: Build (Derleme)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve kütüphaneleri yükle
COPY ["DietitianApp.csproj", "./"]
RUN dotnet restore "DietitianApp.csproj"

# Tüm dosyaları kopyala ve yayınla
COPY . .
RUN dotnet publish "DietitianApp.csproj" -c Release -o /app/publish

# 2. Aşama: Runtime (Çalıştırma)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render'ın beklediği port ayarı
ENV ASPNETCORE_URLS=http://+:10000

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "DietitianApp.dll"]