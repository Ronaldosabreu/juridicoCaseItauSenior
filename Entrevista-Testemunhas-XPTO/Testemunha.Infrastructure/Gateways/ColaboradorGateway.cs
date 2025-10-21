using Testemunha.Domain.Abstractions;
using Testemunha.Domain.Entities;
using Testemunha.Infrastructure.External;
using Testemunha.Infrastructure.Mapper;

namespace Testemunha.Infrastructure.Gateways
{
    public class ColaboradorGateway : IColaboradorGateway
    {
        private readonly IColaboradoresApi _api;
        public ColaboradorGateway(IColaboradoresApi api) => _api = api;

        public async Task<ReclamanteEntity> BuscarCPFAsync(string cpf, CancellationToken ct)
        {
            List<ReclamanteDto> dtos = await _api.GetColaboradoresAsync(cpf, ct);

            ReclamanteEntity retorno = dtos.FirstOrDefault().ToDomain();

            retorno.SaveToMemory();

            return retorno;
        }

        public async Task<List<ReclamanteEntity>> BuscarColaboradoresMemoriaAsync()
        {
            return ColaboradorExtensions.GetAll();
        }
    }
}