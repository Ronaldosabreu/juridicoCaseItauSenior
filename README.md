# 🧩 Entrevista-Testemunhas-XPTO

Projeto desenvolvido em **.NET 8**, com base na **Clean Architecture**, simulando o fluxo de envio de link de entrevistas de testemunhas

O objetivo é demonstrar **boas práticas de arquitetura**, **separação de camadas**, **injeção de dependência** e **testabilidade**, em um contexto **100% local**, utilizando **armazenamento in-memory**.

---

## 🎯 Objetivo da Solução

Simular o fluxo de entrevistas de colaboradores:
1. Um **Reclamante** possui vários **Colaboradores**.
2. O sistema envia um link (simulado) para que colaboradores respondam à entrevista.
3. Tudo é armazenado e consultado **em memória**.
4. Estrutura preparada para futura migração para **infraestrutura AWS** (SQS, DynamoDB, Step Functions, SES).

---

## 🏗️ Estrutura da Solução

src/
├─ Testemunha.API/ → Interface HTTP (Controllers, Swagger, Program.cs)
├─ Testemunha.Application/ → Casos de Uso (UseCases)
├─ Testemunha.Domain/ → Entidades, Enums e Interfaces de Gateway
├─ Testemunha.Infrastructure/ → Implementações de Gateways, Mappers, APIs externas e Injeção de Dependência
└─ tests/ → (opcional) testes unitários e de integração

### 📂 Principais Componentes

| Camada | Pasta | Responsabilidade |
|--------|--------|------------------|
| **API** | `Controllers` | Exposição dos endpoints (`ColaboradoresController`, `EntrevistasController`) |
| **Application** | `UseCases` | Contém as regras de aplicação (`BuscarColaboradoresCPFUseCase`, `EnviarEntrevistasUseCase`) |
| **Domain** | `Abstractions`, `Entities`, `Enum` | Núcleo do sistema — define contratos e entidades | e Reras de negócio
| **Infrastructure** | `Gateways`, `Mappers`, `ExternalAPIs` | Implementa contratos do domínio e manipula dados (in-memory no demo) |

---

## ⚙️ Fluxo de Execução

Controller (API)
↓
UseCase (Application)
↓
Interface Gateway (Domain)
↓
Gateway (Infrastructure)
↓
Mapper → Entity → Resposta HTTP

swift
Copiar código

**Exemplo de fluxo:**  
`ColaboradoresController` → `BuscarColaboradoresCPFUseCase` → `IColaboradorGateway` → `ColaboradorGateway` → dados em memória.

---

## 🔗 Endpoints (exemplos)

| Método | Rota | Descrição |
|--------|------|------------|
| `GET` | `/api/colaboradores/{cpf}` | Busca colaborador por CPF |
| `GET` | `/api/colaboradores/memoria` | Lista colaboradores armazenados em memória |
| `POST` | `/api/entrevistas/enviar` | Simula envio de entrevistas para colaboradores |

### 🧾 Exemplo de resposta
```json
{
  "nome": "Ronaldo Abreu",
  "cpf": "33433417890",
  "racf": "rabreu",
  "email": "ronaldo.abreu@empresa.com",
  "cargo": "Desenvolvedor .NET Sênior",
  "status": "ativo",
  "colaboradores": [
    {
      "nome": "Joice Souza",
      "cpf": "98765432100",
      "email": "joice.souza@empresa.com",
      "cargo": "Analista de QA",
      "status": "ativo"
    },
    {
      "nome": "Beatriz Lima",
      "cpf": "55566677788",
      "email": "beatriz.lima@empresa.com",
      "cargo": "UX Designer",
      "status": "ativo"
    }
  ]
}
🧠 Decisões de Design
Clean Architecture → desacoplamento total entre regras e infraestrutura.

In-Memory Repository → simula base de dados para o demo.

Use Cases isolados → cada caso de uso é testável e possui responsabilidade única.

Gateways e Interfaces → permitem substituição da infraestrutura sem alterar o domínio.

Mappers → garantem independência entre DTOs e Entities.


🧩 Extensibilidade

Infra poderá futuramente incluir:

DynamoDB (armazenamento)

SQS (fila de disparo)

Step Functions (orquestração)

SES (envio de e-mail)

EventBridge (reenvio e expiração de links)

☁️ Evolução para AWS (roadmap futuro)
Recurso AWS	Função
DynamoDB	Guardar status das entrevistas e expiração dos links
SQS + DLQ	Fila de envio e reprocessamento automático
Step Functions	Orquestração do fluxo de envio e expiração
SES	Disparo de e-mails
EventBridge	Scheduler para reenvios e notificações
CloudWatch / Datadog	Monitoramento, métricas e alertas