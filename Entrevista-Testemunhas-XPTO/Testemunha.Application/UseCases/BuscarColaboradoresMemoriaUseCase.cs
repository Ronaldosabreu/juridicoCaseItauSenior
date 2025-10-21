using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testemunha.Domain.Abstractions;
using Testemunha.Domain.Entities;

namespace Testemunha.Application.UseCases
{
  public class BuscarColaboradoresMemoriaUseCase
  {
    private readonly IColaboradorGateway _gateway;
    public BuscarColaboradoresMemoriaUseCase(IColaboradorGateway gateway) => _gateway = gateway;

    public async Task<List<ReclamanteEntity>> ExecuteAsync()
    {
      var colaboradores = await _gateway.BuscarColaboradoresMemoriaAsync();

      return colaboradores;
    }
  }
}