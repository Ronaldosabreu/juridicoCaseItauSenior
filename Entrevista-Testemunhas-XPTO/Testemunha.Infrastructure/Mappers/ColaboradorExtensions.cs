using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testemunha.Domain.Entities;
using Testemunha.Domain.Enum;
using Testemunha.Infrastructure.External;

namespace Testemunha.Infrastructure.Mapper
{
  public static class ColaboradorExtensions
  {
    private static readonly List<ReclamanteEntity> _cache = new();

    public static void SaveToMemory(this ReclamanteEntity reclamante)
    {
      if (!_cache.Any(r => r.Cpf == reclamante.Cpf))
        _cache.Add(reclamante);
    }

    public static List<ReclamanteEntity> GetAll()
    {
      return _cache.ToList();
    }


    public static List<ReclamanteEntity> GetAllPendente()
    {

      var pendentes = _cache
        .Where(r => r.Colaboradores.Any(c => c.StatusEnvio == null))
        .ToList();

      return pendentes;
    }

    public static async Task<bool> BulkUpdateStatus(List<ReclamanteEntity> reclamantesAtualizados)
    {
      if (reclamantesAtualizados is null || reclamantesAtualizados.Count == 0)
        return false;

      int totalAtualizados = 0;

      foreach (var reclamante in reclamantesAtualizados)
      {
        foreach (var colaboradores in reclamante.Colaboradores)
        {
          colaboradores.StatusEnvio = "Enviado";
          colaboradores.HoraEnvio = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
      }

      return true;
    }
  }
}