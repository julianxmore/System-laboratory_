# Imagen base de ASP.NET Core 8 para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Imagen SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos todo el contenido desde la raíz del proyecto
COPY . .

# Restauramos y publicamos el proyecto principal
RUN dotnet restore "Horario_prueba.csproj"
RUN dotnet publish "Horario_prueba.csproj" -c Release -o /app/publish

# Imagen final: solo runtime + binarios publicados
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENTRYPOINT ["dotnet", "Horario_prueba.dll"]
