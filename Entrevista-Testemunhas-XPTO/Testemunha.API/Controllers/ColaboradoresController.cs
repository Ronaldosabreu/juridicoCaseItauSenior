using Microsoft.AspNetCore.Mvc;
using Testemunha.Application.UseCases;

namespace Testemunha.API.Controllers;

[ApiController]
[Route("api/[controller]/reclamantes")]
public sealed class ColaboradoresController : ControllerBase
{
      private readonly BuscarColaboradoresCPFUseCase _buscarColaboradoresUseCase;
  private readonly BuscarColaboradoresMemoriaUseCase _buscarColaboradoresMemoriaUseCase;

  public ColaboradoresController(BuscarColaboradoresCPFUseCase buscarColaboradoresUseCase,
  BuscarColaboradoresMemoriaUseCase buscarColaboradoresMemoriaUseCase
  )
  {
    _buscarColaboradoresUseCase = buscarColaboradoresUseCase;
    _buscarColaboradoresMemoriaUseCase = buscarColaboradoresMemoriaUseCase;
  }

  [HttpGet("{cpf}")]
  public async Task<IActionResult> GetByCpf(string cpf, CancellationToken ct)
  {
    var c = await _buscarColaboradoresUseCase.ExecuteAsync(cpf, ct);
    return c is null ? NotFound() : Ok(c);
  }

  [HttpGet("memoria/")]
  public async Task<IActionResult> GetColaboradoresMemoria()
  {
    var c = await _buscarColaboradoresMemoriaUseCase.ExecuteAsync();
    return c is null ? NotFound() : Ok(c);
  }

}