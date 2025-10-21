using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Testemunha.Application.UseCases;

namespace Testemunha.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class EntrevistasController : ControllerBase
  {

    private readonly EnviarEntrevistasUseCase _buscarColaboradoresMemoriaUseCase;

    public EntrevistasController(EnviarEntrevistasUseCase buscarColaboradoresMemoriaUseCase)
    {
      _buscarColaboradoresMemoriaUseCase = buscarColaboradoresMemoriaUseCase;
    }


    [HttpPost()]
    public async Task<IActionResult> EnviarEntrevista()
    {
        _buscarColaboradoresMemoriaUseCase.ExecuteAsync();

      return Ok();
    }

    //[HttpGet()]
    //public async Task<IActionResult> EntevistasEnviadas()
    //{
    //  return Ok();
    //}



  }
}
