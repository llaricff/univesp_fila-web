using FilaWeb.Api.Endpoints;
using FilaWeb.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS: quais endereços de front-end podem chamar esta API pelo navegador.
// Configure em appsettings.json ("Cors:Origens").
var origens = builder.Configuration.GetSection("Cors:Origens").Get<string[]>()
              ?? ["http://localhost:5173"];
builder.Services.AddCors(opcoes => opcoes.AddPolicy("front", politica =>
    politica.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddSingleton(TimeProvider.System);

// Onde a fila é guardada. Para trocar por um banco de dados, registre aqui outra
// implementação de IFilaRepository.
builder.Services.AddSingleton<IFilaRepository>(sp =>
{
    var relativo = sp.GetRequiredService<IConfiguration>()["Fila:ArquivoDados"];
    var ambiente = sp.GetRequiredService<IHostEnvironment>();
    var caminho = string.IsNullOrWhiteSpace(relativo)
        ? null // sem caminho = só em memória
        : Path.Combine(ambiente.ContentRootPath, relativo);
    return new JsonFilaRepository(caminho);
});

var app = builder.Build();

app.UseCors("front");

app.MapGet("/", () => Results.Ok(new { servico = "Fila Web API", status = "ok" }));
app.MapFilaEndpoints();

app.Run();
