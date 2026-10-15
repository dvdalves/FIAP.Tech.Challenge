# ADR 002: Padrão de Comunicação e Roteamento via AWS API Gateway

| Metadado | Detalhe |
| :--- | :--- |
| **Status** | **Aprovado** |
| **Data** | Outubro / 2026 |
| **Decisores** | Arquiteto de Software, Especialista em Redes e Tech Lead |
| **Contexto** | Requisito Obrigatório da Fase 3 - Autenticação e API Gateway |

---

## 1. Contexto

Com a expansão da oficina para múltiplas unidades, a aplicação precisa expor endpoints públicos para clientes (agendamento, aprovação de orçamentos e consulta de status de OS) e endpoints para os operadores e mecânicos da oficina.

Expor o cluster Kubernetes diretamente para a Internet através de NodePorts ou Load Balancers públicos sem uma camada de borda cria riscos:
- Falta de controle centralizado de taxas de requisição (*throttling*) e proteção contra ataques DoS;
- Dificuldade para unificar a autenticação de rotas serverless (Lambda) e rotas conteinerizadas (EKS);
- Falta de padronização de CORS e rastreabilidade nos acessos externos.

A **Fase 3** exige a implementação de um **API Gateway** (AWS API Gateway, Kong, Traefik ou similar) para proteção e controle de tráfego.

---

## 2. Decisão

Adotamos o **AWS API Gateway (HTTP API v2)** como a porta de entrada única (*Single Point of Entry / Reverse Proxy*) para todas as requisições externas do sistema SIAES.

### Padrão de Roteamento Definido:
1. **Rotas de Autenticação (`/auth/*`)**:
   - `POST /auth/token` e `POST /auth/login` são roteadas diretamente para a **AWS Lambda de Autenticação** via integração nativa de proxy (`AWS_PROXY`, Payload v2.0).
2. **Rotas Operacionais e de Negócio (`/api/*`)**:
   - Roteadas para a aplicação principal .NET 10 hospedada no cluster Kubernetes EKS.
   - A comunicação é viabilizada através de um **AWS API Gateway VPC Link** apontando para o Application Load Balancer (ALB) ou Ingress Controller interno, mantendo os nós worker em subnets privadas sem exposição pública de IP.
3. **Controle de CORS Padronizado**:
   - Habilitação de cabeçalhos CORS centralizados no API Gateway (`GET, POST, PUT, DELETE, OPTIONS`, cabeçalhos `Authorization`, `Content-Type`, `traceparent`).
4. **Access Logs Estruturados no CloudWatch**:
   - Emissão de logs de acesso em formato JSON contendo `requestId`, `sourceIp`, `requestTime`, `httpMethod`, `routeKey`, `status`, `integrationLatency` e `responseLatency`.

---

## 3. Consequências

### Consequências Positivas
- **Isolamento de Infraestrutura**: Nenhum nó do EKS possui IP público. O acesso à API principal ocorre exclusivamente através do canal cifrado do API Gateway.
- **Roteamento Híbrido Serverless / Contêiner**: Clientes externos consomem um único domínio base para autenticar e consumir serviços, sem saber se o backend é uma Lambda ou um Pod Kubernetes.
- **Auditoria e Observabilidade na Borda**: Todas as chamadas geram métricas no CloudWatch (`IntegrationLatency`, `Count`, `5XXError`, `4XXError`), permitindo alarmes em tempo real.

### Consequências Negativas / Mitigações
- **Latência de Integração**: A camada de API Gateway adiciona uma sobrecarga mínima de processamento (típica de 5 a 15 ms). Foi utilizado o tipo HTTP API v2 justamente por ser até 60% mais rápido que o API Gateway REST v1 clássico.
