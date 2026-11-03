# RFC 002: Escolha do Banco de Dados Gerenciado (PostgreSQL 16 no Azure Database for PostgreSQL)

| Metadado | Detalhe |
| :--- | :--- |
| **Status** | **Aprovado** |
| **Data** | Outubro / 2026 |
| **Autor** | Equipe de Arquitetura SIAES - Tech Challenge FIAP |
| **Contexto** | Fase 3 - Modelagem, Consistência e Alta Disponibilidade de Dados |
| **Decisores** | Arquiteto de Software, DBA e Especialista de Domínio |

---

## 1. Contexto e Problema

O sistema da oficina mecânica gerencia dados transacionais de alta relevância contábil e operacional:
- Abertura, cálculo orçamentário e aprovação de **Ordens de Serviço (OS)**;
- Baixa transacional no **estoque físico de peças**;
- Registro de **clientes e veículos** com validação de unicidade de CPF e Placa.

Na Fase 2, o banco de dados era executado em um pod de PostgreSQL com Persistent Volume Claim (PVC) no próprio cluster Kubernetes ou em contêiner local Docker. Para a **Fase 3**, tornou-se obrigatória a adoção de uma solução de **Banco de Dados Gerenciado em Nuvem**, exigindo uma justificativa formal para a tecnologia escolhida e seus respectivos ajustes no modelo relacional.

---

## 2. Tecnologias Avaliadas

| Critério | PostgreSQL 16 (Azure Database for PostgreSQL) | MySQL 8.x (Azure Database for PostgreSQL) | MongoDB Atlas (NoSQL) | Azure DynamoDB (NoSQL) |
| :--- | :--- | :--- | :--- | :--- |
| **Modelo de Consistência** | **ACID Estrito**: Excelente isolamento transacional, controle de concorrência multiversão (MVCC). | **ACID**: Bom suporte via InnoDB, porém menor flexibilidade de tipos complexos. | **Eventual / ACID limitado a documento**: Maior complexidade para transações multi-tabela. | **Eventual / Transacional com restrições**: Custo elevado e modelagem complexa para agregações financeiras. |
| **Integridade Referencial** | **Nativa e Robusta**: Foreign Keys com ON DELETE CASCADE, CHECK constraints e índices parciais. | **Nativa**: Foreign Keys com bom suporte. | Inexistente nativamente; gerenciada na aplicação. | Inexistente nativamente; gerenciada na aplicação. |
| **Tipos de Dados** | **UUID nativo**, NUMERIC de alta precisão, TIMESTAMP WITH TIME ZONE e JSONB. | UUID armazenado como BINARY ou VARCHAR; TIMESTAMP com limites. | BSON com boa tipagem flexível. | Tipos simples (String, Number, Binary). |
| **Compatibilidade ORM** | **EF Core 10 + Npgsql**: Suporte oficial de primeira classe a Value Objects, Conversores e Transações. | EF Core + Pomelo MySQL: Bom suporte. | Requer drivers NoSQL; perda de abstração do EF Core relacional. | Incompatível com o padrão EF Core relacional. |
| **Alta Disponibilidade** | **Azure Database for PostgreSQL zone-redundant**: Réplica síncrona com failover automático e sem perda de dados (RPO=0). | Azure Database for PostgreSQL zone-redundant disponível. | Replica Sets gerenciados no Atlas. | zone-redundant nativo. |

---

## 3. Justificativa da Escolha do PostgreSQL 16 no Azure Database for PostgreSQL

O **PostgreSQL 16 gerenciado via Azure Database for PostgreSQL** foi escolhido com base nos seguintes pilares fundamentais:

1. **Atomicidade e Isolamento em Cenários Críticos de Negócio**:
   - Quando um cliente aprova um orçamento, a transação deve calcular o subtotal de serviços, o total de peças, aplicar a baixa no estoque da tabela `Pecas` e transicionar o status da OS para `EmExecucao`. Se o estoque de qualquer peça for insuficiente, a transação inteira deve sofrer *rollback*. O motor MVCC do PostgreSQL assegura esse isolamento mesmo em concorrência simultânea entre mecânicos e clientes.
2. **Identificadores UUID v4 e Mitigação contra IDOR**:
   - Todas as entidades utilizam `UUID` (gerados pela extensão `uuid-ossp` ou na camada de domínio da aplicação). O PostgreSQL possui tipo nativo de 16 bytes para UUIDs, o que economiza espaço em disco e garante máxima performance de indexação B-Tree sem vulnerabilidade a enumeração sequencial por usuários mal-intencionados.
3. **Índices Otimizados para Alta Performance**:
   - A autenticação via Function Serverless executa buscas frequentes por `Cpf`. A criação de um índice único B-Tree (`IX_Clientes_Cpf`) garante complexidade $O(1)$ na validação, prevenindo varreduras sequenciais de tabela (*Full Table Scan*).
4. **Governança Operacional do Azure Database for PostgreSQL**:
   - Alocação elástica de armazenamento de 20 GiB para até 50 GiB via *Storage Auto-scaling*;
   - Janelas de manutenção automatizadas, backups contínuos com retenção de 7 dias e diagnóstico de consultas lentas através do *Performance Insights*.

---

## 4. Ajustes Realizados no Modelo Relacional (Fase 3)

1. **Normalização até a 3ª Forma Normal (3FN)**:
   - Segregação de `Pecas` e `Servicos` em catálogos distintos com códigos únicos e validações numéricas não-negativas (`CHECK >= 0`).
2. **Tabela Associativa Polimórfica para Itens da OS**:
   - Criação da tabela `ItensOrdemServico`, garantindo que cada item pertença a uma `OrdensServico` e aponte para um `ServicoId` ou `PecaId` com valor unitário histórico preservado no momento da inserção.
3. **Controle de Estoque Mínimo**:
   - Inclusão do campo `EstoqueMinimo` na tabela `Pecas`, permitindo que o sistema sinalize reposições preventivas antes do desabastecimento da oficina.
