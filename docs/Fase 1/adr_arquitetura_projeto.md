# ADR 001: Padrões Arquiteturais, DDD e Persistência do SIAES

## 1. Contexto e Problema

O sistema Oficina Mecânica (SIAES) necessita de um back-end de alta manutenibilidade, seguro contra vulnerabilidades e com facilidade de execução para desenvolvimento local e testes automatizados. O edital exige a modelagem do domínio focado nas ordens de serviço, orçamento automático e acompanhamento do cliente.

---

## 2. Decisão

Adotamos a seguinte estrutura arquitetural e de padrões táticos:

### A. Clean Architecture (Arquitetura Limpa)

A solução é estruturada em 4 camadas de responsabilidades rígidas:

* **Domínio (`OficinaMecanica.Domain`)**: Sem dependências externas. Contém entidades ricas, objetos de valor (Value Objects), exceções de domínio e contratos de repositório.
* **Aplicação (`OficinaMecanica.Application`)**: Orquestra os Casos de Uso, DTOs de entrada/saída e validações estruturais de payloads usando FluentValidation.
* **Infraestrutura (`OficinaMecanica.Infrastructure`)**: Implementa a persistência via Entity Framework Core (mapeamento Fluent API, migrações e repositórios) e serviços transversais (criptografia/tokens).
* **API (`OficinaMecanica.API`)**: Ponto de entrada REST, contendo apenas controllers, filtros de exceção e injeção de dependência.

### B. DDD (Domain-Driven Design) Tático

Para evitar modelos anêmicos e garantir consistência:

* As entidades possuem construtores protegidos, propriedades com modificadores `private set` e validam suas regras de negócio em métodos internos (ex: `AdicionarItem()`).
* A `OrdemServico` atua como a **Raiz do Agregado (Aggregate Root)**. Todas as alterações em itens associados (`ItemOrdemServico`) passam obrigatoriamente pela raiz.
* Encapsulamento de tipos complexos em **Objetos de Valor (Value Objects)** imutáveis (ex: `Cpf`, `PlacaVeiculo`, `Diagnostico`, `Orcamento`).

### C. Estratégia de Persistência Híbrida (Multi-Provider)

Mapeamos o comportamento do banco de dados para três cenários distintos:

1. **Produção**: Banco de dados relacional **PostgreSQL** orquestrado em container via Docker Compose.
2. **Desenvolvimento Local**: Fallback automático para banco de dados **SQLite** em arquivo físico (`oficina.db`), permitindo execução imediata com `dotnet run` sem necessidade de subir dependências externas.
3. **Testes de Integração**: Uso do provedor **In-Memory do Entity Framework Core** com nomes de bancos randômicos por instância (`InMemoryDbForTesting_Guid`). Isso evita conflito de concorrência e poluição de estado entre os testes paralelos.

* **Prevenção a IDOR**: Uso nativo de `Guid` (UUID) para todas as URLs e referências expostas a clientes.
* **Resolução de Vulnerabilidades**: Atualização do motor de SQLite nativo nas dependências para o bundle seguro `SQLitePCLRaw.bundle_e_sqlite3` (v3.0.3) para corrigir a falha de truncamento numérico (CVE-2025-6965).
* **Análise de Qualidade e Cobertura**: Integração com **SonarQube** utilizando `dotnet-sonarscanner` e geração de relatórios de cobertura em formato Cobertura XML (via Coverlet), centralizando as métricas de qualidade de código estático (SAST).

---

## 3. Consequências

* **Isolamento de Regras**: As regras de negócio permanecem imunes a alterações de frameworks de terceiros ou provedores de banco de dados.
* **Agilidade no Desenvolvimento**: Programadores conseguem clonar o repositório e executar a aplicação ou rodar os testes (`dotnet test`) de imediato sem instalar bancos locais.
* **Velocidade de Testes**: A suíte de testes integrados roda em milissegundos com banco isolado em memória.
