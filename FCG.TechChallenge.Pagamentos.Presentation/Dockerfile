# --- build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copie só os csproj (melhor cache)
COPY FCG.TechChallenge.Pagamentos.Domain/FCG.TechChallenge.Pagamentos.Domain.csproj FCG.TechChallenge.Pagamentos.Domain/
COPY FCG.TechChallenge.Pagamentos.Application/FCG.TechChallenge.Pagamentos.Application.csproj FCG.TechChallenge.Pagamentos.Application/
COPY FCG.TechChallenge.Pagamentos.Infrastructure/FCG.TechChallenge.Pagamentos.Infrastructure.csproj FCG.TechChallenge.Pagamentos.Infrastructure/
COPY FCG.TechChallenge.Pagamentos.Presentation/FCG.TechChallenge.Pagamentos.Presentation.csproj FCG.TechChallenge.Pagamentos.Presentation/

RUN dotnet restore FCG.TechChallenge.Pagamentos.Presentation/FCG.TechChallenge.Pagamentos.Presentation.csproj

# agora copie o resto do código
COPY . .

RUN dotnet publish FCG.TechChallenge.Pagamentos.Presentation/FCG.TechChallenge.Pagamentos.Presentation.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# --- runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FCG.TechChallenge.Pagamentos.Presentation.dll"]
