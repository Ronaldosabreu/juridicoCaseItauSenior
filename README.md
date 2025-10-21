# README — Envio de Entrevista para Testemunha/Colaborador

> Projeto em .NET (API) com orquestração em AWS para disparo de entrevistas a colaboradores/testemunhas, captura de respostas e trilha de auditoria.

---

## 1) Visão Geral

Este documento explica **de ponta a ponta** o fluxo de envio de entrevista para colaboradores (também chamados de testemunhas) a partir de um **Controller** .NET. Cobre:

* Modelagem de domínio (Reclamante, Colaborador, Entrevista, Pergunta, Convite, Resposta)
* Endpoints REST (Controller ➜ Application ➜ Domain)
* Mapeamentos (DTO → Domain) e validações
* Disparo assíncrono via **SQS/Step Functions/Lambda** para montar perguntas por cargo e enviar o **link de resposta por e‑mail** (expira em **5 dias úteis**)
* Persistência e eventos (DynamoDB)
* Segurança (token/assinatura de link), expiração, idempotência
* Observabilidade (logs/metrics/alerts), tratamento de erros e DLQ
* Execução local, testes e checklist de produção

---

## 2) Arquitetura (alto nível)

```mermaid
flowchart LR
  A[API .NET Controller
POST /entrevistas/enviar] --> B[Application Service
Validação + Mapeamento]
  B --> C[(DynamoDB
Eventos/Entrevistas)]
  B --> D[SQS
queue-entrevistas]
  D --> E[Step Functions
Orquestração]
  E --> L1[Lambda A
Busca Colaboradores]
  E --> L2[Lambda B
Monta Perguntas por Cargo]
  E --> L3[Lambda C
Gera Link + Envia Email (SES)]
  L3 --> C
  subgraph Resposta
    U[Usuário (Colaborador)] --> W[Frontend Web
/entrevistas/{token}]
    W --> API[API .NET
GET/POST respostas]
    API --> C
  end
```

---

## 2A) Modo Demo Local (100% In‑Memory, **sem token**, **sem DynamoDB**)

> **Objetivo:** demonstrar codificação end‑to‑end usando apenas memória de processo. Links usam **`entrevistaId`** em vez de token. Nada de filas, orquestração ou banco.

```mermaid
flowchart LR
  A[API .NET Controller
POST /entrevistas/enviar] --> B[Service
Validação + Mapeamento]
  B --> C[(InMemoryStore
Reclamantes/Colaboradores/Entrevistas/Convites)]
  C --> D[Simulador de Envio
(loga link: /entrevistas/{entrevistaId})]
  subgraph Resposta
    U[Colaborador] --> W[Frontend local
/entrevistas/{entrevistaId}]
    W --> API[API .NET
GET perguntas / POST respostas]
    API --> C
  end
```

### Estruturas em Memória

```csharp
public static class InMemoryStore
{
  public static readonly ConcurrentDictionary<string, Reclamante> Reclamantes = new(); // cpf -> Reclamante
  public static readonly ConcurrentDictionary<string, Colaborador> Colaboradores = new(); // cpf -> Colaborador
  public static readonly ConcurrentDictionary<string, Entrevista> Entrevistas = new();   // entrevistaId -> Entrevista
}
```

### Modelo (mínimo) — Domain puro

```csharp
public class Reclamante
{
  public string Nome { get; }
  public string Cpf { get; }
  public string Racf { get; }
  public string Email { get; }
  public string Cargo { get; }
  public string Status { get; }
  public List<Colaborador> Colaboradores { get; } = new();
  public Reclamante(string nome, string cpf, string racf, string email, string cargo, string status, IEnumerable<Colaborador>? colabs = null)
  { Nome = nome; Cpf = cpf; Racf = racf; Email = email; Cargo = cargo; Status = status; if (colabs != null) Colaboradores.AddRange(colabs); }
}

public enum StatusColaborador { Ativo, Inativo }
public class Colaborador
{
  public string Nome { get; private set; }
  public string Cpf { get; }
  public string Racf { get; private set; }
  public string Email { get; private set; }
  public string Cargo { get; private set; }
  public StatusColaborador Status { get; private set; }
  public void Atualizar(string? nome=null, string? racf=null, string? email=null, string? cargo=null, StatusColaborador? status=null)
  { if (!string.IsNullOrWhiteSpace(nome)) Nome=nome; if (!string.IsNullOrWhiteSpace(racf)) Racf=racf; if (!string.IsNullOrWhiteSpace(email)) Email=email; if (!string.IsNullOrWhiteSpace(cargo)) Cargo=cargo; if (status.HasValue) Status=status.Value; }
  public Colaborador(string nome, string cpf, string racf, string email, string cargo, StatusColaborador status)
  { Nome=nome; Cpf=cpf; Racf=racf; Email=email; Cargo=cargo; Status=status; }
}

public record Pergunta(string Id, string Texto, string CargoAlvo);
public record Resposta(string PerguntaId, string Valor, DateTimeOffset RespondidaEm);

public class Entrevista
{
  public string Id { get; }
  public string ProcessoId { get; }
  public string ColaboradorCpf { get; }
  public DateTimeOffset CriadaEm { get; } = DateTimeOffset.UtcNow;
  public string Status { get; private set; } = "Criada"; // Criada, Enviada, Respondida
  public List<Pergunta> Perguntas { get; } = new();
  public List<Resposta> Respostas { get; } = new();
  public Entrevista(string id, string processoId, string colaboradorCpf, IEnumerable<Pergunta> perguntas)
  { Id=id; ProcessoId=processoId; ColaboradorCpf=colaboradorCpf; Perguntas.AddRange(perguntas); }
  public void MarcarEnviada() => Status = "Enviada";
  public void MarcarRespondida() => Status = "Respondida";
}
```

### DTOs + Mapper

```csharp
public class ColaboradorDto { public string Nome { get; set; } = default!; public string Cpf { get; set; } = default!; public string Racf { get; set; } = default!; public string Email { get; set; } = default!; public string Cargo { get; set; } = default!; public string Status { get; set; } = default!; }
public static class ColaboradorMapper
{
  public static Colaborador ToDomain(this ColaboradorDto dto)
  {
    var status = dto.Status.Equals("ativo", StringComparison.OrdinalIgnoreCase) ? StatusColaborador.Ativo : StatusColaborador.Inativo;
    return new Colaborador(dto.Nome, dto.Cpf, dto.Racf, dto.Email, dto.Cargo, status);
  }
}
```

### Serviço (demo)

```csharp
public class EntrevistaService
{
  private readonly ILogger<EntrevistaService> _logger;
  public EntrevistaService(ILogger<EntrevistaService> logger) => _logger = logger;

  public (string correlationId, int convites) Enviar(EnviarEntrevistasRequest req)
  {
    req.Validate();
    var reclamante = new Reclamante(req.Reclamante.Nome, req.Reclamante.Cpf, req.Reclamante.Racf, req.Reclamante.Email, req.Reclamante.Cargo, req.Reclamante.Status, req.Colaboradores.Select(c => c.ToDomain()));

    InMemoryStore.Reclamantes.AddOrUpdate(reclamante.Cpf, reclamante, (_,__) => reclamante);
    foreach (var c in reclamante.Colaboradores)
      InMemoryStore.Colaboradores.AddOrUpdate(c.Cpf, c, (_, old) => { old.Atualizar(nome:c.Nome, racf:c.Racf, email:c.Email, cargo:c.Cargo, status:c.Status); return old; });

    var correlationId = Guid.NewGuid().ToString("N");
    foreach (var c in reclamante.Colaboradores.Where(x => x.Status == StatusColaborador.Ativo))
    {
      var perguntas = PerguntasPorCargo(c.Cargo);
      var entrevista = new Entrevista(Guid.NewGuid().ToString("N"), req.ProcessoId, c.Cpf, perguntas);
      entrevista.MarcarEnviada();
      InMemoryStore.Entrevistas[entrevista.Id] = entrevista;
      _logger.LogInformation("[SIMULADO] Envio para {Email} com link: http://localhost:5072/entrevistas/{Id}", c.Email, entrevista.Id);
    }
    return (correlationId, reclamante.Colaboradores.Count);
  }

  public static List<Pergunta> PerguntasPorCargo(string cargo) => new()
  {
    new Pergunta("P1", $"Descreva suas atividades como {cargo}.", cargo),
    new Pergunta("P2", "Houve testemunho de terceiros?", cargo)
  };
}
```

### Controllers (demo)

```csharp
[ApiController]
[Route("api/entrevistas")] 
public class EntrevistasController : ControllerBase
{
  private readonly EntrevistaService _service;
  public EntrevistasController(EntrevistaService service) => _service = service;

  [HttpPost("enviar")]
  public IActionResult Enviar([FromBody] EnviarEntrevistasRequest req)
  {
    var (correlationId, convites) = _service.Enviar(req);
    return Accepted(new { req.ProcessoId, quantidadeConvitesCriados = convites, correlationId, status = "Processando-Local" });
  }

  [HttpGet]
  public IActionResult Listar() => Ok(InMemoryStore.Entrevistas.Values.Select(e => new { e.Id, e.ProcessoId, e.ColaboradorCpf, e.Status }));

  [HttpGet("{entrevistaId}")]
  public IActionResult Perguntas(string entrevistaId)
  {
    if (!InMemoryStore.Entrevistas.TryGetValue(entrevistaId, out var ent)) return NotFound();
    return Ok(new { ent.Id, ent.ProcessoId, perguntas = ent.Perguntas.Select(p => new { p.Id, p.Texto }) });
  }

  public record RespostasRequest(List<Item> Respostas) { public record Item(string PerguntaId, string Valor); }

  [HttpPost("{entrevistaId}/respostas")]
  public IActionResult Responder(string entrevistaId, [FromBody] RespostasRequest body)
  {
    if (!InMemoryStore.Entrevistas.TryGetValue(entrevistaId, out var ent)) return NotFound();
    foreach (var r in body.Respostas) ent.Respostas.Add(new Resposta(r.PerguntaId, r.Valor, DateTimeOffset.UtcNow));
    ent.MarcarRespondida();
    return Ok(new { mensagem = "Respostas registradas" });
  }
}

[ApiController]
[Route("api/colaboradores")] 
public class ColaboradoresController : ControllerBase
{
  [HttpGet]
  public IActionResult Listar() => Ok(InMemoryStore.Colaboradores.Values.Select(c => new { c.Nome, c.Cpf, c.Email, c.Cargo, c.Status }));

  public record UpdateColabDto(string? Nome, string? Racf, string? Email, string? Cargo, string? Status);

  [HttpPut("{cpf}")]
  public IActionResult Atualizar(string cpf, [FromBody] UpdateColabDto dto)
  {
    if (!InMemoryStore.Colaboradores.TryGetValue(cpf, out var c)) return NotFound();
    StatusColaborador? s = dto.Status == null ? null : dto.Status.Equals("ativo", StringComparison.OrdinalIgnoreCase) ? StatusColaborador.Ativo : StatusColaborador.Inativo;
    c.Atualizar(dto.Nome, dto.Racf, dto.Email, dto.Cargo, s);
    return Ok(new { mensagem = "Atualizado", c.Nome, c.Email, c.Cargo, c.Status });
  }
}
```

### Program.cs (mínimo)

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<EntrevistaService>();
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
```

### Endpoints úteis (demo)

* `POST /api/entrevistas/enviar` — cria entrevistas e **loga** o link `/entrevistas/{entrevistaId}`.
* `GET /api/entrevistas` — lista entrevistas criadas.
* `GET /api/entrevistas/{entrevistaId}` — retorna perguntas.
* `POST /api/entrevistas/{entrevistaId}/respostas` — registra respostas.
* `GET /api/colaboradores` — lista colaboradores do in‑memory.
* `PUT /api/colaboradores/{cpf}` — atualiza dados do colaborador em memória.
