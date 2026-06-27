# Requisitos Funcionais e Não Funcionais - Sistema Oficina Mecânica

Este documento descreve detalhadamente os **Requisitos Funcionais (RF)** e **Requisitos Não Funcionais (RNF)** para o MVP (Minimum Viable Product) do **Sistema Integrado de Atendimento e Execução de Serviços** da Oficina Mecânica, projetado seguindo as diretrizes de **DDD (Domain-Driven Design)**, **Clean Architecture** e segurança.

---

## 📌 Requisitos Funcionais (RF)

Os requisitos funcionais definem os fluxos de trabalho, regras de negócio e ações que o sistema deve executar para atender às necessidades da oficina e de seus clientes.

### 1. Cadastro e Gestão de Cadastros Base (CRUD)

| ID | Requisito | Descrição | Prioridade |
| :--- | :--- | :--- | :--- |
| **RF-01** | **Gestão de Clientes** | O sistema deve permitir cadastrar, consultar, atualizar e inativar clientes. Os campos obrigatórios são: Nome/Razão Social, CPF/CNPJ, E-mail e Telefone.<br>⚠️ *Validação:* O CPF/CNPJ deve ser estruturalmente válido no momento do cadastro. | Alta |
| **RF-02** | **Gestão de Veículos** | O sistema deve permitir cadastrar, consultar, atualizar e inativar veículos. Cada veículo deve estar obrigatoriamente associado a um cliente (proprietário) e possuir Placa, Marca, Modelo e Ano de Fabricação.<br>⚠️ *Validação:* A placa do veículo deve seguir as validações do padrão Mercosul e nacional vigente. | Alta |
| **RF-03** | **Gestão de Peças e Insumos** | O sistema deve gerenciar o cadastro de peças e insumos em estoque, contendo descrição, código de referência único, valor unitário de venda e quantidade disponível para uso. | Alta |
| **RF-04** | **Gestão de Serviços (Mão de Obra)** | O sistema deve manter o catálogo de tipos de serviços oferecidos (ex: alinhamento, balanceamento, troca de óleo), registrando descrição do serviço e valor padrão de mão de obra. | Alta |

### 2. Fluxo e Ciclo de Vida da Ordem de Serviço (OS)

| ID | Requisito | Descrição | Prioridade |
| :--- | :--- | :--- | :--- |
| **RF-05** | **Criação da Ordem de Serviço (Abertura)** | O sistema deve permitir a abertura de uma nova Ordem de Serviço (OS) associando um cliente (identificado por CPF/CNPJ) e seu respectivo veículo. O atendente deve registrar a descrição do problema relatado pelo cliente. O status inicial da OS é **Recebida**. | Alta |
| **RF-06** | **Diagnóstico e Orçamento** | O sistema deve permitir que o mecânico adicione itens ao orçamento da OS (peças cadastradas com quantidade e serviços com base no catálogo). O valor total da OS deve ser calculado automaticamente somando os insumos e os valores de mão de obra. O status da OS muda para **Em diagnóstico**. | Alta |
| **RF-07** | **Envio e Aprovação do Orçamento** | O sistema deve disponibilizar o orçamento gerado para a aprovação do cliente. O status da OS passa para **Aguardando aprovação**. O cliente poderá, por meio do sistema, visualizar a proposta financeira e aprovar ou rejeitar os reparos. | Alta |
| **RF-08** | **Execução da Ordem de Serviço** | Após a aprovação do cliente, o status da OS é alterado para **Em execução**. Ao entrar neste status, o estoque de peças e insumos incluídos na OS é atualizado automaticamente (baixa de estoque). | Alta |
| **RF-09** | **Finalização do Serviço** | Ao terminar as tarefas mecânicas, a OS deve ser marcada como **Finalizada**, gravando-se automaticamente a data e hora do encerramento dos trabalhos. | Alta |
| **RF-10** | **Entrega do Veículo** | O sistema deve permitir registrar a saída física do veículo da oficina, alterando o status da OS para **Entregue**. | Média |

### 3. Acompanhamento e Métricas de Gestão

| ID | Requisito | Descrição | Prioridade |
| :--- | :--- | :--- | :--- |
| **RF-11** | **Acompanhamento do Status pelo Cliente** | O sistema deve expor um endpoint público que permita ao cliente acompanhar o progresso de sua OS em tempo real (consultando por identificador/código da OS e placa do veículo), exibindo o status atual e detalhes públicos dos serviços. | Alta |
| **RF-12** | **Monitoramento do Tempo Médio de Execução** | O sistema deve computar e exibir o tempo médio decorrido entre o início da execução (OS alterada para "Em execução") e a conclusão dos serviços (OS alterada para "Finalizada"). | Média |

---

## 🔒 Requisitos Não Funcionais (RNF)

Os requisitos não funcionais especificam critérios técnicos, padrões de qualidade, restrições e atributos de sistema que garantem o bom funcionamento e a manutenibilidade do software.

### 1. Arquitetura e Engenharia de Software

| ID | Requisito | Detalhamento |
| :--- | :--- | :--- |
| **RNF-01** | **Monolito Estruturado** | O sistema deve ser implementado como um monolito modular estruturado em camadas no .NET (ex: `API`, `Application`, `Domain`, `Infrastructure`), garantindo separação rígida de conceitos. |
| **RNF-02** | **Domain-Driven Design (DDD)** | A lógica de negócios deve residir no **Domínio**, que deve ser agnóstico de tecnologias e infraestrutura de persistência. Entidades de domínio devem ter comportamento rico, validando regras internas e proibindo estado anêmico através de propriedades com `private set`. |
| **RNF-03** | **Inversão de Dependências** | As camadas externas (API e Infraestrutura) dependem de abstrações (interfaces) definidas no Domínio e Aplicação, e nunca o contrário. |

### 2. Tecnologia e Persistência

| ID | Requisito | Detalhamento |
| :--- | :--- | :--- |
| **RNF-04** | **Plataforma de Desenvolvimento** | O backend deve ser desenvolvido em **C#** utilizando a plataforma **.NET 10** (ou versão recente estável aplicável). |
| **RNF-05** | **Persistência Relacional (EF Core)** | A persistência deve ser feita em banco de dados relacional (ex: PostgreSQL ou SQL Server). O mapeamento das entidades deve ser isolado na camada de infraestrutura via Fluent API do **Entity Framework Core**, evitando poluição no domínio. |
| **RNF-06** | **Containerização** | A aplicação e o banco de dados devem ser orquestrados via contêineres. Deve ser fornecido um **Dockerfile** multi-stage otimizado para build do app e um arquivo **docker-compose.yml** para levantar o ambiente completo de forma simplificada. |

### 3. Segurança e Confiabilidade

| ID | Requisito | Detalhamento |
| :--- | :--- | :--- |
| **RNF-07** | **Segurança nas APIs Administrativas (JWT)** | As operações administrativas (CRUDs de clientes, veículos, estoque e faturamento) devem ser protegidas por autenticação via token **JWT (JSON Web Token)** utilizando esquema Bearer. |
| **RNF-08** | **Preservação de IDs de Banco (UUID/Guid)** | Para evitar falhas do tipo IDOR (Insecure Direct Object Reference), todas as URIs expostas publicamente devem usar identificadores do tipo **Guid (UUID)**, evitando expor IDs sequenciais do banco de dados na URL. |
| **RNF-09** | **Tratamento de Exceções Global** | O sistema deve possuir um filtro global de exceções (`Global Exception Filter`) para interceptar erros. Exceções de regras de negócio (`DominioException`) devem ser retornadas como `400 Bad Request` ou `422 Unprocessable Entity` com mensagens amigáveis, sem expor stack traces de infraestrutura. |

### 4. Qualidade e Documentação

| ID | Requisito | Detalhamento |
| :--- | :--- | :--- |
| **RNF-10** | **Documentação OpenAPI (Swagger)** | As APIs RESTful do sistema devem ser documentadas via Swagger, fornecendo as definições de rotas, payloads, schemas e suporte a envio de cabeçalhos de autenticação JWT Bearer. |
| **RNF-11** | **Cobertura de Testes Automatizados** | Os domínios críticos e fluxos de negócio do sistema devem possuir no mínimo **80% de cobertura de testes unitários** e de integração, utilizando frameworks como xUnit, NSubstitute e FluentAssertions. |
| **RNF-12** | **Processamento Assíncrono** | Todas as chamadas de E/S (operações de banco de dados, requisições de rede, etc.) devem ser implementadas usando `async` / `await` para otimizar o uso de threads do servidor e suportar cancelamento via `CancellationToken`. |
