namespace Testemunha.Domain.Entities
{
  public class ColaboradorEntity
{
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string Racf { get; set; }
    public string Email { get; set; }
    public string Cargo { get; set; }
    public string Status { get; set; }
    public string StatusEnvio { get; set; } = null;
    public string HoraEnvio { get; set; }


    public ColaboradorEntity()
    {

    }

    public ColaboradorEntity(string nome, string cpf, string racf, string cargo, string email, string status, string horaEnvio)
    {
      Nome = nome;
      Cpf = cpf;
      Racf = racf;
      Cargo = cargo;
      Email = email;
      Status = status;

      ValidarDominio();
      HoraEnvio = horaEnvio;
    }

    private void ValidarDominio()
    {
      if (string.IsNullOrWhiteSpace(Nome))
        throw new ArgumentException("O nome do colaborador é obrigatório.");

      if (Cpf.Length != 11)
        throw new ArgumentException("CPF inválido.");

      if (string.IsNullOrWhiteSpace(Email))
        throw new ArgumentException("E-mail inválido.");
    }
  }
}
