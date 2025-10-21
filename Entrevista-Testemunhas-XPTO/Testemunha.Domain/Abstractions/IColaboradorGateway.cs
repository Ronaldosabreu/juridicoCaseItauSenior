using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testemunha.Domain.Entities;

namespace Testemunha.Domain.Abstractions
{
  public interface IColaboradorGateway
  {
    Task<ReclamanteEntity> BuscarCPFAsync(string cpf, CancellationToken ct);

    Task<List<ReclamanteEntity>> BuscarColaboradoresMemoriaAsync();
  }  
}
