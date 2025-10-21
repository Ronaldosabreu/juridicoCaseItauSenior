using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Refit;
using System.Net;
using Testemunha.Domain.Abstractions;
using Testemunha.Infrastructure.External;
using Testemunha.Infrastructure.Gateways;

namespace Testemunha.Infrastructure.DependencyInjection
{
  public static class RefitPollySetup
  {
    public static IServiceCollection AddExternalApis(this IServiceCollection services, IConfiguration cfg)
    {
      var collabBase = cfg["ExternalApis:Colaboradores"] ?? throw new InvalidOperationException("Missing Colaboradores base URL");
      var scriptsBase = cfg["ExternalApis:Scripts"] ?? throw new InvalidOperationException("Missing Scripts base URL");


      var jitter = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(200), 5);

      var retry = HttpPolicyExtensions
          .HandleTransientHttpError()
          .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
          .WaitAndRetryAsync(jitter);

      var breaker = HttpPolicyExtensions
          .HandleTransientHttpError()
          .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(30));

      var timeout = Policy.TimeoutAsync<HttpResponseMessage>(10);


      services
          .AddRefitClient<IColaboradoresApi>()
          .ConfigureHttpClient(c => c.BaseAddress = new Uri(collabBase))
          .AddPolicyHandler(retry)
          .AddPolicyHandler(breaker)
          .AddPolicyHandler(timeout);


      // Gateways
      services.AddScoped<IColaboradorGateway, ColaboradorGateway>();
      services.AddScoped<IEntrevistasGateway, EntrevistaGateway>();
      
      return services;
    }
  }
}
