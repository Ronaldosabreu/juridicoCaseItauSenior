using Testemunha.Domain.Entities;

namespace Testemunha.Domain.Abstractions
{
  public interface IEntrevistasGateway
  {
    Task<bool> EnviarEntrevista();

    Task<List<ReclamanteEntity>> BuscarEntrevistasEnviadas();
  }
}
