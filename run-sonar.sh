#!/usr/bin/env bash
# ==============================================================================
# Script de Automação de Análise do SonarQube com Relatório de Cobertura (.NET)
# ==============================================================================
set -e

SONAR_URL=${SONAR_HOST_URL:-"http://localhost:9000"}
SONAR_PROJECT_KEY="FIAP.Tech.Challenge"
SONAR_PROJECT_NAME="Oficina Mecanica - SIAES"

# 1. Solicitar ou ler o Token do SonarQube
if [ -z "$SONAR_TOKEN" ]; then
    echo "⚠️  SONAR_TOKEN não definido como variável de ambiente."
    read -rp "👉 Digite o Token de Acesso do seu projeto SonarQube: " input_token
    SONAR_TOKEN=$input_token
    if [ -z "$SONAR_TOKEN" ]; then
        echo "❌ Erro: O Token do SonarQube é obrigatório para realizar a análise."
        exit 1
    fi
fi

# 2. Verificar se o Java está instalado (pré-requisito do SonarScanner)
if ! command -v java &> /dev/null; then
    echo "❌ Erro: Java (JRE/JDK) é necessário para executar o SonarScanner. Por favor, instale o Java."
    exit 1
fi

# 3. Verificar/Instalar a ferramenta dotnet-sonarscanner
echo "🔍 Verificando dotnet-sonarscanner..."
if ! dotnet tool list -g | grep -q "dotnet-sonarscanner"; then
    echo "📦 Instalando dotnet-sonarscanner globalmente..."
    dotnet tool install --global dotnet-sonarscanner
else
    echo "✅ dotnet-sonarscanner já está instalado."
fi

# 4. Limpar diretórios temporários de testes anteriores
echo "🧹 Limpando resultados de testes anteriores..."
rm -rf **/TestResults

# 5. Garantir que o SonarQube está online
echo "📡 Verificando conexão com o SonarQube em $SONAR_URL..."
for i in {1..30}; do
    STATUS_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$SONAR_URL/api/system/status" || true)
    if [ "$STATUS_CODE" -eq 200 ]; then
        echo "✅ SonarQube está online."
        break
    fi
    echo "⏳ Aguardando inicialização do SonarQube (tentativa $i de 30)..."
    sleep 3
done

# 6. Iniciar o Scanner do SonarQube
echo "🚀 Iniciando análise do SonarQube..."
dotnet sonarscanner begin \
    /k:"$SONAR_PROJECT_KEY" \
    /n:"$SONAR_PROJECT_NAME" \
    /d:sonar.host.url="$SONAR_URL" \
    /d:sonar.token="$SONAR_TOKEN" \
    /d:sonar.cs.opencover.reportsPaths="**/TestResults/*/coverage.opencover.xml" \
    /d:sonar.coverage.exclusions="tests/**,src/FIAP.Tech.Challenge.API/Migrations/**" \
    /d:sonar.qualitygate.wait=true

# 7. Compilar o Projeto
echo "⚙️  Compilando a solução..."
dotnet build --no-incremental

# 8. Executar Testes Coletando Cobertura (formato OpenCover via coverlet.runsettings)
echo "🧪 Executando testes automatizados com cobertura..."
dotnet test --no-build --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# 9. Encerrar o Scanner e enviar relatórios
echo "📤 Concluindo análise e enviando resultados para o SonarQube..."
dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"

echo "🎉 Análise concluída com sucesso! Verifique o painel do SonarQube em: $SONAR_URL"
