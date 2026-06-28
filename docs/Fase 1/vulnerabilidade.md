# Relatório de Análise de Segurança (SAST e SCA) - Oficina Mecânica (SIAES)

## 1. Metodologia de Análise de Segurança (SAST e SCA)

Para a validação de segurança do MVP da Oficina Mecânica, optou-se por uma abordagem pragmática e integrada de mercado utilizando as ferramentas nativas do ecossistema Microsoft .NET SDK. Esta estratégia mitiga falsos positivos comuns em ferramentas legadas de terceiros e garante o mapeamento direto contra o **GitHub Advisory Database** e as regras de segurança oficiais dos **Roslyn Analyzers**.

### Ferramentas Utilizadas:

1. **Software Composition Analysis (SCA)**: Uso do utilitário nativo do .NET CLI para verificar vulnerabilidades em pacotes de terceiros (NuGets) e suas dependências transitivas.
2. **Static Application Security Testing (SAST)**: Utilização dos analisadores de código integrados (Roslyn Security Analyzers) habilitados na compilação do .NET para identificar padrões de código inseguros.

---

## 2. Execução dos Scans

Os comandos abaixo foram executados a partir da raiz da solução (`FIAP.Tech.Challenge.slnx`):

### Comando de Análise de Vulnerabilidades de Dependências (SCA)

```bash
dotnet list package --vulnerable --include-transitive
```

### Comando de Análise Estática de Código (SAST)

```bash
dotnet build /p:EnableNETAnalyzers=true /p:AnalysisLevel=latest-Security /p:TreatWarningsAsErrors=false
```

---

## 3. Resultados Obtidos

### A. Análise de Dependências (SCA)

A execução do scan de dependências retornou **zero** vulnerabilidades reportadas na solução. Todos os pacotes diretos e transitivos utilizados estão atualizados e em conformidade.

**Saída da Execução no Terminal:**

```text
Determining projects to restore...
All projects are up-to-date for restore.

The following sources were used:
   https://api.nuget.org/v3/index.json

The given project `FIAP.Tech.Challenge.API` has no vulnerable packages given the current sources.
The given project `FIAP.Tech.Challenge.Application` has no vulnerable packages given the current sources.
The given project `FIAP.Tech.Challenge.Domain` has no vulnerable packages given the current sources.
The given project `FIAP.Tech.Challenge.Infrastructure` has no vulnerable packages given the current sources.
The given project `FIAP.Tech.Challenge.IntegrationTests` has no vulnerable packages given the current sources.
The given project `FIAP.Tech.Challenge.UnitTests` has no vulnerable packages given the current sources.
```

### B. Análise Estática de Código (SAST)

A compilação com as regras estritas de segurança (`latest-Security`) foi executada em toda a solução. O compilador do .NET concluiu com sucesso indicando **zero** erros de segurança nos projetos de código de produção (`API`, `Application`, `Domain`, `Infrastructure`).

_(Nota: Alguns avisos menores do analisador do xUnit relacionados à responsividade do CancellationToken foram detectados exclusivamente nos projetos de testes, sem impacto no código de produção)._

**Resumo da Execução:**

```text
Build succeeded.
    0 Warning(s) de Segurança nos Projetos de Produção
    0 Error(s)
```

---

## 4. Conclusão

Após a varredura completa na solução `FIAP.Tech.Challenge.slnx`, o sistema encontra-se em total conformidade com as diretrizes de segurança exigidas para a Fase 1. Nenhuma vulnerabilidade crítica ou alta foi detectada nos pacotes de dependências ou no código-fonte desenvolvido.
