# API Reference - SIAES (Oficina Mecânica)

Esta documentação descreve todos os endpoints públicos e administrativos do **SIAES (Sistema Integrado de Atendimento e Execução de Serviços)** da Oficina Mecânica.

---

## 🔑 Autenticação

<details>
<summary>
 Geração de Token JWT (Teste)
</summary>

**Método:** POST  
**URI:** `/api/public/auth/token`

**Parâmetros (Query):**

- `usuario` (optional): `string` (Nome ou Guid do usuário. Default: `"admin"`)
- `perfil` (optional): `string` (`Admin`, `Mecanico` ou `Cliente`. Default: `"Admin"`)

**Exemplo request:**

```bash
curl -X POST "http://localhost:8080/api/public/auth/token?usuario=admin&perfil=Admin"
```

Resposta: **200 OK**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

</details>

---

## 👤 Clientes e Veículos (Admin)

Todos os endpoints administrativos de clientes exigem o cabeçalho `Authorization: Bearer <TOKEN_JWT>` com perfil **Admin**.

<details>
<summary>
 Cadastrar novo cliente
</summary>

**Método:** POST  
**URI:** `/api/admin/clientes`

**Exemplo request:**

```bash
curl -X POST "http://localhost:8080/api/admin/clientes" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"nome":"João da Silva","cpf":"12345678909","email":"joao@email.com","telefone":"11988887777"}'
```

Resposta: **210 Created**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nome": "João da Silva",
  "cpf": "12345678909",
  "email": "joao@email.com",
  "telefone": "11988887777"
}
```

</details>

<details>
<summary>
 Listar todos os clientes com frotas
</summary>

**Método:** GET  
**URI:** `/api/admin/clientes`

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/clientes" \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

Resposta: **200 OK**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "João da Silva",
    "cpf": {
      "valor": "12345678909"
    },
    "email": "joao@email.com",
    "telefone": "11988887777",
    "veiculos": [
      {
        "id": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
        "placa": {
          "valor": "ABC1D23"
        },
        "marca": "Ford",
        "modelo": "Focus",
        "ano": 2018
      }
    ]
  }
]
```

</details>

<details>
<summary>
 Obter cliente por ID
</summary>

**Método:** GET  
**URI:** `/api/admin/clientes/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID do cliente)

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/clientes/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

Resposta: **200 OK**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nome": "João da Silva",
  "cpf": {
    "valor": "12345678909"
  },
  "email": "joao@email.com",
  "telefone": "11988887777",
  "veiculos": []
}
```

</details>

<details>
<summary>
 Vincular veículo à frota do cliente
</summary>

**Método:** POST  
**URI:** `/api/admin/clientes/{id}/veiculos`

**Parâmetros:**

- `id` (path, required): `Guid` (ID do cliente proprietário)

**Exemplo request:**

```bash
curl -X POST "http://localhost:8080/api/admin/clientes/3fa85f64-5717-4562-b3fc-2c963f66afa6/veiculos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"placa":"ABC1D23","marca":"Ford","modelo":"Focus","ano":2018}'
```

Resposta: **200 OK**

```json
{
  "id": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
  "placa": "ABC1D23",
  "marca": "Ford",
  "modelo": "Focus",
  "ano": 2018
}
```

</details>

<details>
<summary>
 Atualizar cliente
</summary>

**Método:** PUT  
**URI:** `/api/admin/clientes/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID do cliente)

**Exemplo request:**

```bash
curl -X PUT "http://localhost:8080/api/admin/clientes/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"nome":"João da Silva Atualizado","email":"joao.novo@email.com","telefone":"11999998888"}'
```

Resposta: **200 OK**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nome": "João da Silva Atualizado",
  "cpf": "12345678909",
  "email": "joao.novo@email.com",
  "telefone": "11999998888"
}
```

</details>

<details>
<summary>
 Excluir cliente
</summary>

**Método:** DELETE  
**URI:** `/api/admin/clientes/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID do cliente)

**Exemplo request:**

```bash
curl -X DELETE "http://localhost:8080/api/admin/clientes/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

Resposta: **204 No Content**

_⚠️ Nota: A exclusão é impedida caso o cliente possua veículos vinculados ou ordens de serviço associadas (retorna erro HTTP 400)._

</details>

<details>
<summary>
 Listar todos os veículos
</summary>

**Método:** GET  
**URI:** `/api/admin/veiculos`

**Parâmetros (Query):**

- `clienteId` (optional): `Guid` (Filtra veículos por cliente proprietário)

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/veiculos" \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

Resposta: **200 OK**

```json
[
  {
    "id": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
    "placa": "ABC1D23",
    "marca": "Ford",
    "modelo": "Focus",
    "ano": 2018,
    "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }
]
```

</details>

<details>
<summary>
 Obter veículo por ID
</summary>

**Método:** GET  
**URI:** `/api/admin/veiculos/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID do veículo)

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/veiculos/7ca85f64-5717-4562-b3fc-2c963f66afb2" \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

Resposta: **200 OK**

```json
{
  "id": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
  "placa": "ABC1D23",
  "marca": "Ford",
  "modelo": "Focus",
  "ano": 2018,
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

</details>

<details>
<summary>
 Atualizar veículo
</summary>

**Método:** PUT  
**URI:** `/api/admin/veiculos/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID do veículo)

**Exemplo request:**

```bash
curl -X PUT "http://localhost:8080/api/admin/veiculos/7ca85f64-5717-4562-b3fc-2c963f66afb2" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"placa":"XYZ9D87","marca":"Ford","modelo":"Focus Novo","ano":2019}'
```

Resposta: **200 OK**

```json
{
  "id": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
  "placa": "XYZ9D87",
  "marca": "Ford",
  "modelo": "Focus Novo",
  "ano": 2019,
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

</details>

<details>
<summary>
 Excluir veículo
</summary>

**Método:** DELETE  
**URI:** `/api/admin/veiculos/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID do veículo)

**Exemplo request:**

```bash
curl -X DELETE "http://localhost:8080/api/admin/veiculos/7ca85f64-5717-4562-b3fc-2c963f66afb2" \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

Resposta: **204 No Content**

_⚠️ Nota: A exclusão é impedida caso o veículo possua ordens de serviço vinculadas (retorna erro HTTP 400)._

</details>

---

## ⚙️ Catálogo de Peças e Estoque (Admin)

Endpoints administrativos de peças exigem o cabeçalho `Authorization: Bearer <TOKEN_JWT>`.

<details>
<summary>
 Listar estoque de peças (Acesso: Mecanico, Admin)
</summary>

**Método:** GET  
**URI:** `/api/admin/pecas`

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/pecas" \
  -H "Authorization: Bearer <TOKEN_MECANICO_OU_ADMIN>"
```

Resposta: **200 OK**

```json
[
  {
    "id": "5ca85f64-5717-4562-b3fc-2c963f66afc4",
    "nome": "Pastilha de Freio Dianteira",
    "preco": 189.9,
    "quantidadeEstoque": 15
  }
]
```

</details>

<details>
<summary>
 Cadastrar nova peça (Acesso: Admin)
</summary>

**Método:** POST  
**URI:** `/api/admin/pecas`

**Exemplo request:**

```bash
curl -X POST "http://localhost:8080/api/admin/pecas" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"nome":"Pastilha de Freio Dianteira","preco":189.90,"quantidadeEstoque":15}'
```

Resposta: **201 Created**

```json
{
  "id": "5ca85f64-5717-4562-b3fc-2c963f66afc4",
  "nome": "Pastilha de Freio Dianteira",
  "preco": 189.9,
  "quantidadeEstoque": 15
}
```

</details>

<details>
<summary>
 Ajustar saldo em estoque (Acesso: Admin)
</summary>

**Método:** PUT  
**URI:** `/api/admin/pecas/{id}/estoque`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da peça)
- `quantidade` (query, required): `int` (Novo saldo físico em estoque)

**Exemplo request:**

```bash
curl -X PUT "http://localhost:8080/api/admin/pecas/5ca85f64-5717-4562-b3fc-2c963f66afc4/estoque?quantidade=20" \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

Resposta: **200 OK**

```json
{
  "mensagem": "Estoque atualizado com sucesso."
}
```

</details>

<details>
<summary>
 Obter peça por ID (Acesso: Mecanico, Admin)
</summary>

**Método:** GET  
**URI:** `/api/admin/pecas/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da peça)

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/pecas/5ca85f64-5717-4562-b3fc-2c963f66afc4" \
  -H "Authorization: Bearer <TOKEN_MECANICO_OU_ADMIN>"
```

Resposta: **200 OK**

```json
{
  "id": "5ca85f64-5717-4562-b3fc-2c963f66afc4",
  "nome": "Pastilha de Freio Dianteira",
  "preco": 189.9,
  "quantidadeEstoque": 15
}
```

</details>

<details>
<summary>
 Atualizar peça (Acesso: Admin)
</summary>

**Método:** PUT  
**URI:** `/api/admin/pecas/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da peça)

**Exemplo request:**

```bash
curl -X PUT "http://localhost:8080/api/admin/pecas/5ca85f64-5717-4562-b3fc-2c963f66afc4" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"nome":"Pastilha de Freio Dianteira Atualizada","preco":199.90}'
```

Resposta: **200 OK**

```json
{
  "id": "5ca85f64-5717-4562-b3fc-2c963f66afc4",
  "nome": "Pastilha de Freio Dianteira Atualizada",
  "preco": 199.9,
  "quantidadeEstoque": 15
}
```

</details>

<details>
<summary>
 Excluir peça (Acesso: Admin)
</summary>

**Método:** DELETE  
**URI:** `/api/admin/pecas/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da peça)

**Exemplo request:**

```bash
curl -X DELETE "http://localhost:8080/api/admin/pecas/5ca85f64-5717-4562-b3fc-2c963f66afc4" \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

Resposta: **204 No Content**

_⚠️ Nota: A exclusão é impedida caso a peça esteja associada a algum item de ordem de serviço (retorna erro HTTP 400)._

</details>

---

## 🛠️ Catálogo de Serviços (Admin)

Endpoints administrativos do catálogo de serviços exigem o cabeçalho `Authorization: Bearer <TOKEN_JWT>`.

<details>
<summary>
 Listar catálogo de serviços (Acesso: Mecanico, Admin)
</summary>

**Método:** GET  
**URI:** `/api/admin/servicos`

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/servicos" \
  -H "Authorization: Bearer <TOKEN_MECANICO_OU_ADMIN>"
```

Resposta: **200 OK**

```json
[
  {
    "id": "6ca85f64-5717-4562-b3fc-2c963f66afd5",
    "descricao": "Alinhamento e Balanceamento",
    "precoMaoDeObra": 120.0
  }
]
```

</details>

<details>
<summary>
 Cadastrar novo serviço (Acesso: Admin)
</summary>

**Método:** POST  
**URI:** `/api/admin/servicos`

**Exemplo request:**

```bash
curl -X POST "http://localhost:8080/api/admin/servicos" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"descricao":"Alinhamento e Balanceamento","precoMaoDeObra":120.00}'
```

Resposta: **201 Created**

```json
{
  "id": "6ca85f64-5717-4562-b3fc-2c963f66afd5",
  "descricao": "Alinhamento e Balanceamento",
  "precoMaoDeObra": 120.0
}
```

</details>

<details>
<summary>
 Obter serviço por ID (Acesso: Mecanico, Admin)
</summary>

**Método:** GET  
**URI:** `/api/admin/servicos/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID do serviço)

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/servicos/6ca85f64-5717-4562-b3fc-2c963f66afd5" \
  -H "Authorization: Bearer <TOKEN_MECANICO_OU_ADMIN>"
```

Resposta: **200 OK**

```json
{
  "id": "6ca85f64-5717-4562-b3fc-2c963f66afd5",
  "descricao": "Alinhamento e Balanceamento",
  "precoMaoDeObra": 120.0
}
```

</details>

<details>
<summary>
 Atualizar serviço (Acesso: Admin)
</summary>

**Método:** PUT  
**URI:** `/api/admin/servicos/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID do serviço)

**Exemplo request:**

```bash
curl -X PUT "http://localhost:8080/api/admin/servicos/6ca85f64-5717-4562-b3fc-2c963f66afd5" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"descricao":"Alinhamento Completo","precoMaoDeObra":150.00}'
```

Resposta: **200 OK**

```json
{
  "id": "6ca85f64-5717-4562-b3fc-2c963f66afd5",
  "descricao": "Alinhamento Completo",
  "precoMaoDeObra": 150.0
}
```

</details>

<details>
<summary>
 Excluir serviço (Acesso: Admin)
</summary>

**Método:** DELETE  
**URI:** `/api/admin/servicos/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID do serviço)

**Exemplo request:**

```bash
curl -X DELETE "http://localhost:8080/api/admin/servicos/6ca85f64-5717-4562-b3fc-2c963f66afd5" \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

Resposta: **204 No Content**

_⚠️ Nota: A exclusão é impedida caso o serviço esteja associado a alguma ordem de serviço (retorna erro HTTP 400)._

</details>

---

## 📋 Ordens de Serviço (Admin)

Endpoints para controle e montagem da Ordem de Serviço exigem o cabeçalho `Authorization: Bearer <TOKEN_JWT>`.

<details>
<summary>
 Abertura de OS (Acesso: Admin)
</summary>

**Método:** POST  
**URI:** `/api/admin/ordens-servico`

**Exemplo request:**

```bash
curl -X POST "http://localhost:8080/api/admin/ordens-servico" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_ADMIN>" \
  -d '{"clienteId":"3fa85f64-5717-4562-b3fc-2c963f66afa6","veiculoId":"7ca85f64-5717-4562-b3fc-2c963f66afb2","descricaoProblema":"Vazamento de óleo e barulho na suspensão."}'
```

Resposta: **201 Created**

```json
{
  "id": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "veiculoId": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
  "descricaoProblema": "Vazamento de óleo e barulho na suspensão.",
  "status": "Recebida",
  "itens": [],
  "valorTotal": 0
}
```

</details>

<details>
<summary>
 Listar Ordens de Serviço (Acesso: Mecanico, Admin)
</summary>

**Método:** GET  
**URI:** `/api/admin/ordens-servico`

**Parâmetros (Query):**

- `status` (optional): `int` (Status da OS: 0 a 6)
- `clienteId` (optional): `Guid` (ID do cliente)

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/ordens-servico?status=0" \
  -H "Authorization: Bearer <TOKEN_MECANICO_OU_ADMIN>"
```

Resposta: **200 OK**

```json
[
  {
    "id": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
    "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "veiculoId": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
    "descricaoProblema": "Vazamento de óleo e barulho na suspensão.",
    "status": "Recebida",
    "itens": [],
    "valorTotal": 0
  }
]
```

</details>

<details>
<summary>
 Detalhar Ordem de Serviço por ID (Acesso: Mecanico, Admin)
</summary>

**Método:** GET  
**URI:** `/api/admin/ordens-servico/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da Ordem de Serviço)

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/ordens-servico/9ca85f64-5717-4562-b3fc-2c963f66afe6" \
  -H "Authorization: Bearer <TOKEN_MECANICO_OU_ADMIN>"
```

Resposta: **200 OK**

```json
{
  "id": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "veiculoId": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
  "descricaoProblema": "Vazamento de óleo e barulho na suspensão.",
  "status": "Recebida",
  "itens": [],
  "valorTotal": 0
}
```

</details>

<details>
<summary>
 Lançar diagnóstico e itens na OS (Acesso: Mecanico, Admin)
</summary>

**Método:** POST  
**URI:** `/api/admin/ordens-servico/{id}/itens`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da Ordem de Serviço)

**Exemplo request:**

```bash
curl -X POST "http://localhost:8080/api/admin/ordens-servico/9ca85f64-5717-4562-b3fc-2c963f66afe6/itens" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_MECANICO_OU_ADMIN>" \
  -d '{"itensPeca":[{"pecaId":"5ca85f64-5717-4562-b3fc-2c963f66afc4","quantidade":2}],"itensServico":[{"servicoId":"6ca85f64-5717-4562-b3fc-2c963f66afd5"}]}'
```

Resposta: **200 OK**

```json
{
  "id": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "veiculoId": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
  "descricaoProblema": "Vazamento de óleo e barulho na suspensão.",
  "status": "AguardandoAprovacao",
  "itens": [
    {
      "tipo": "Peca",
      "referenciaId": "5ca85f64-5717-4562-b3fc-2c963f66afc4",
      "nome": "Pastilha de Freio Dianteira",
      "quantidade": 2,
      "precoUnitario": 189.9,
      "valorTotal": 379.8
    },
    {
      "tipo": "Servico",
      "referenciaId": "6ca85f64-5717-4562-b3fc-2c963f66afd5",
      "nome": "Alinhamento e Balanceamento",
      "quantidade": 1,
      "precoUnitario": 120.0,
      "valorTotal": 120.0
    }
  ],
  "valorTotal": 499.8
}
```

</details>

<details>
<summary>
 Transicionar status da OS (Acesso: Mecanico, Admin)
</summary>

**Método:** PUT  
**URI:** `/api/admin/ordens-servico/{id}/status`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da Ordem de Serviço)

**Exemplo request (Mudar para Finalizada):**

```bash
curl -X PUT "http://localhost:8080/api/admin/ordens-servico/9ca85f64-5717-4562-b3fc-2c963f66afe6/status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN_MECANICO_OU_ADMIN>" \
  -d '{"novoStatus":4}'
```

_⚠️ Nota: Para transicionar para o status **`Entregue`** (novoStatus = 5), o token deve possuir a role **Admin**._

Resposta: **200 OK**

```json
{
  "id": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
  "status": "Finalizada",
  "dataInicioExecucao": "2026-06-28T16:00:00Z",
  "dataFinalizacao": "2026-06-28T17:15:00Z",
  "valorTotal": 499.8
}
```

</details>

<details>
<summary>
 Obter tempo médio de execução das OSs finalizadas (Acesso: Admin)
</summary>

**Método:** GET  
**URI:** `/api/admin/ordens-servico/metricas/tempo-medio`

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/ordens-servico/metricas/tempo-medio" \
  -H "Authorization: Bearer <TOKEN_ADMIN>"
```

Resposta: **200 OK**

```json
{
  "tempoMedioHoras": 1.25,
  "totalOrdensFinalizadas": 12
}
```

</details>

<details>
<summary>
 Consulta pontual de status da OS (Acesso: Mecanico, Admin)
</summary>

**Método:** GET  
**URI:** `/api/admin/ordens-servico/{id}/status`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da Ordem de Serviço)

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/admin/ordens-servico/9ca85f64-5717-4562-b3fc-2c963f66afe6/status" \
  -H "Authorization: Bearer <TOKEN_MECANICO_OU_ADMIN>"
```

Resposta: **200 OK**

```json
{
  "ordemServicoId": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
  "status": "EmExecucao",
  "descricaoStatus": "Em Execução / Manutenção",
  "valorTotal": 499.8,
  "dataCriacao": "2026-08-26T19:00:00Z",
  "dataInicioExecucao": "2026-08-26T19:30:00Z",
  "dataFinalizacao": null
}
```

</details>

<details>
<summary>
 Disparar notificação manual por e-mail ao cliente (Acesso: Mecanico, Admin)
</summary>

**Método:** POST  
**URI:** `/api/admin/ordens-servico/{id}/notificar`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da Ordem de Serviço)

**Exemplo request:**

```bash
curl -X POST "http://localhost:8080/api/admin/ordens-servico/9ca85f64-5717-4562-b3fc-2c963f66afe6/notificar" \
  -H "Authorization: Bearer <TOKEN_MECANICO_OU_ADMIN>"
```

Resposta: **200 OK**

```json
{
  "mensagem": "Notificação de e-mail enviada com sucesso para o cliente.",
  "email": "cliente@email.com"
}
```

</details>

---

## 📱 Fluxo do Cliente (Público)

Todos os endpoints abaixo exigem o cabeçalho `Authorization: Bearer <TOKEN_JWT>`, exceto as consultas públicas e webhooks com assinatura/identificador.

<details>
<summary>
 Obter Ordens de Serviço ativas do cliente (Ordenadas por status e data)
</summary>

**Método:** GET  
**URI:** `/api/public/ordens-servico`

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/public/ordens-servico" \
  -H "Authorization: Bearer <TOKEN_CLIENTE>"
```

Resposta: **200 OK**

```json
[
  {
    "id": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
    "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "veiculoId": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
    "descricaoProblema": "Vazamento de óleo e barulho na suspensão.",
    "status": "AguardandoAprovacao",
    "itens": [
      {
        "tipo": "Peca",
        "nome": "Pastilha de Freio Dianteira",
        "quantidade": 2,
        "precoUnitario": 189.9,
        "valorTotal": 379.8
      }
    ],
    "valorTotal": 379.8
  }
]
```

</details>

<details>
<summary>
 Consultar status público da OS (Recebida, Diagnóstico, Aguardando Aprovação, Execução, Finalizada, Entregue)
</summary>

**Método:** GET  
**URI:** `/api/public/ordens-servico/{id}/status`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da Ordem de Serviço)

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/public/ordens-servico/9ca85f64-5717-4562-b3fc-2c963f66afe6/status"
```

Resposta: **200 OK**

```json
{
  "ordemServicoId": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
  "status": "AguardandoAprovacao",
  "descricaoStatus": "Aguardando Aprovação do Cliente",
  "valorTotal": 379.8,
  "dataCriacao": "2026-08-26T18:00:00Z",
  "dataInicioExecucao": null,
  "dataFinalizacao": null
}
```

</details>

<details>
<summary>
 Acompanhar OS específica por ID
</summary>

**Método:** GET  
**URI:** `/api/public/ordens-servico/{id}`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da Ordem de Serviço)

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/api/public/ordens-servico/9ca85f64-5717-4562-b3fc-2c963f66afe6" \
  -H "Authorization: Bearer <TOKEN_CLIENTE>"
```

_⚠️ Nota de Segurança: Apenas o proprietário da Ordem de Serviço pode visualizá-la. Tentativas de acessar a OS de outro cliente resultarão em **`403 Forbidden`**._

Resposta: **200 OK**

```json
{
  "id": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
  "clienteId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "veiculoId": "7ca85f64-5717-4562-b3fc-2c963f66afb2",
  "descricaoProblema": "Vazamento de óleo e barulho na suspensão.",
  "status": "AguardandoAprovacao",
  "itens": [
    {
      "tipo": "Peca",
      "nome": "Pastilha de Freio Dianteira",
      "quantidade": 2,
      "precoUnitario": 189.9,
      "valorTotal": 379.8
    }
  ],
  "valorTotal": 379.8
}
```

</details>

<details>
<summary>
 Aprovar orçamento da OS
</summary>

**Método:** POST  
**URI:** `/api/public/ordens-servico/{id}/aprovar`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da Ordem de Serviço)

**Exemplo request:**

```bash
curl -X POST "http://localhost:8080/api/public/ordens-servico/9ca85f64-5717-4562-b3fc-2c963f66afe6/aprovar" \
  -H "Authorization: Bearer <TOKEN_CLIENTE>"
```

_⚠️ Nota: Transiciona o status da OS para `EmExecucao`, abate de forma atômica o estoque físico de insumos e notifica o cliente por e-mail._

Resposta: **200 OK**

```json
{
  "id": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
  "status": "EmExecucao",
  "valorTotal": 379.8
}
```

</details>

<details>
<summary>
 Rejeitar orçamento da OS
</summary>

**Método:** POST  
**URI:** `/api/public/ordens-servico/{id}/rejeitar`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da Ordem de Serviço)

**Exemplo request:**

```bash
curl -X POST "http://localhost:8080/api/public/ordens-servico/9ca85f64-5717-4562-b3fc-2c963f66afe6/rejeitar" \
  -H "Authorization: Bearer <TOKEN_CLIENTE>"
```

_⚠️ Nota: Cancela a OS (status transiciona para `Cancelada`) e dispara notificação por e-mail._

Resposta: **200 OK**

```json
{
  "id": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
  "status": "Cancelada",
  "valorTotal": 379.8
}
```

</details>

<details>
<summary>
 Notificação externa / Webhook de aprovação ou recusa do orçamento
</summary>

**Método:** POST  
**URI:** `/api/public/ordens-servico/{id}/notificacao-orcamento`

**Parâmetros:**

- `id` (path, required): `Guid` (ID da Ordem de Serviço)

**Exemplo request (Aprovação):**

```bash
curl -X POST "http://localhost:8080/api/public/ordens-servico/9ca85f64-5717-4562-b3fc-2c963f66afe6/notificacao-orcamento" \
  -H "Content-Type: application/json" \
  -d '{"aprovado":true,"observacao":"Aprovado pelo cliente via integração externa/chatbot."}'
```

Resposta: **200 OK**

```json
{
  "id": "9ca85f64-5717-4562-b3fc-2c963f66afe6",
  "status": "EmExecucao",
  "valorTotal": 379.8
}
```

</details>

---

## 🩺 Monitoramento & Health Check

<details>
<summary>
 Health Check da Aplicação (Kubernetes Liveness & Readiness Probes)
</summary>

**Método:** GET  
**URI:** `/health`

**Exemplo request:**

```bash
curl -X GET "http://localhost:8080/health"
```

Resposta: **200 OK**

```text
Healthy
```

</details>

