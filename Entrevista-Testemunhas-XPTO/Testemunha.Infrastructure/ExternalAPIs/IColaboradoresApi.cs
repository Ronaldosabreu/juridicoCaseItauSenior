using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Refit;
using Testemunha.Domain.Entities;

namespace Testemunha.Infrastructure.External
{
  public interface IColaboradoresApi
  {
    [Get("/reclamantes")]
    Task<List<ReclamanteDto>> GetColaboradoresAsync(string cpf, CancellationToken ct);
  }


  public class ReclamanteDto
  {
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string Racf { get; set; }
    public string Email { get; set; }
    public string Cargo { get; set; }
    public string Status { get; set; }

    public List<ColaboradorEntity> Colaboradores { get; set; } = new();
  }

  public class ColaboradorDto
  {
    public string nome { get; set; }
    public string cpf { get; set; }
    public string racf { get; set; }
    public string cargo { get; set; }
    public string email { get; set; }
    public string status { get; set; }
  }

}