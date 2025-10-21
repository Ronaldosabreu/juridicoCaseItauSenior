using Testemunha.Domain.Abstractions;
using Testemunha.Domain.Entities;

namespace Testemunha.Application.UseCases
{
  public class BuscarColaboradoresCPFUseCase
  {
    private readonly IColaboradorGateway _gateway;
    public BuscarColaboradoresCPFUseCase(IColaboradorGateway gateway) => _gateway = gateway;

    public async Task<ReclamanteEntity> ExecuteAsync(string cpf, CancellationToken ct)
    {
      var colaboradores = await _gateway.BuscarCPFAsync(cpf, ct);
      return colaboradores;
    }
  }
}