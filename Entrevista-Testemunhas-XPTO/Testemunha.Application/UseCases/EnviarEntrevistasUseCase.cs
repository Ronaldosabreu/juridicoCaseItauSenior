using Testemunha.Domain.Abstractions;
using Testemunha.Domain.Entities;

namespace Testemunha.Application.UseCases
{
  public class EnviarEntrevistasUseCase
  {

    private readonly IEntrevistasGateway _gateway;

    public EnviarEntrevistasUseCase(IEntrevistasGateway colaboradorGateway)
    {
      _gateway= colaboradorGateway;
    }

    public async Task<bool> ExecuteAsync()
    {
      return await _gateway.EnviarEntrevista();
    }
  }
}
