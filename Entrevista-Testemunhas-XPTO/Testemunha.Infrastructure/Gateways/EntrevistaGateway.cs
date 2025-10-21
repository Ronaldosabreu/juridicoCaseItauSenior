using Testemunha.Domain.Abstractions;
using Testemunha.Domain.Entities;
using Testemunha.Infrastructure.Mapper;

namespace Testemunha.Infrastructure.Gateways
{
  public class EntrevistaGateway : IEntrevistasGateway
  {
    public Task<List<ReclamanteEntity>> BuscarEntrevistasEnviadas()
    {
        

      throw new NotImplementedException();
    }

    public async Task<bool> EnviarEntrevista()
    {
      List<ReclamanteEntity> colaboradoresPataEntrevista = ColaboradorExtensions.GetAllPendente();

      return await ColaboradorExtensions.BulkUpdateStatus(colaboradoresPataEntrevista);
    }
  }
}
