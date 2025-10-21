using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testemunha.Domain.Entities
{

  public class ReclamanteEntity
  {
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string Racf { get; set; }
    public string Email { get; set; }
    public string Cargo { get; set; }
    public string Status { get; set; }
    
    public List<ColaboradorEntity> Colaboradores { get; set; } = new();

    public ReclamanteEntity()
    {
    }

    public ReclamanteEntity(
        string nome,
        string cpf,
        string racf,
        string email,
        string cargo,
        string status,
        List<ColaboradorEntity>? colaboradores = null)
    {
      Nome = nome;
      Cpf = cpf;
      Racf = racf;
      Email = email;
      Cargo = cargo;
      Status = status;
      Colaboradores = colaboradores ?? new List<ColaboradorEntity>();
    }

  }


}
