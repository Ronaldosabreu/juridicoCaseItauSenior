using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testemunha.Domain.Entities;
using Testemunha.Domain.Enum;
using Testemunha.Infrastructure.External;

namespace Testemunha.Infrastructure.Mapper
{
  public static class ColaboradorMapper
  {
    public static ReclamanteEntity ToDomain(this ReclamanteDto dto)
    {
      var colaboradores = dto.Colaboradores?
          .Select(c => new ColaboradorEntity(
              nome: c.Nome,
              cpf: c.Cpf,
              racf: c.Racf,
              email: c.Email,
              cargo: c.Cargo,
              status: c.Status.Equals("ativo", StringComparison.OrdinalIgnoreCase) ? "ativo" : "inativo",
              horaEnvio: c.HoraEnvio
          ))
          .ToList();

      return new ReclamanteEntity(
          nome: dto.Nome,
          cpf: dto.Cpf,
          racf: dto.Racf,
          email: dto.Email,
          cargo: dto.Cargo,
          status: dto.Status.Equals("ativo", StringComparison.OrdinalIgnoreCase) ? "ativo" : "inativo",
          colaboradores: colaboradores
      );
    }
  }
}
