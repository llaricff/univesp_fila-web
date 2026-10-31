using System.Security.Cryptography;
using System.Text;
using FilaWeb.Api.Models;
using FilaWeb.Api.Services;

namespace FilaWeb.Api.Endpoints;

public static class FilaEndpoints
{
    public static void MapFilaEndpoints(this IEndpointRouteBuilder app)
    {
        MapPaciente(app.MapGroup("/api/fila"));
        MapEquipe(app.MapGroup("/api/atendimento"));
    }

    // =====================================================================
    // Área pública: usada pela tela de cadastro e de acompanhamento
    // =====================================================================
    private static void MapPaciente(RouteGroupBuilder fila)
    {
        // POST /api/fila  -> entra na fila
        fila.MapPost("/", async (CadastroRequest? req, IFilaRepository repo, TimeProvider tempo) =>
        {
            if (!CadastroValidator.TryValidar(req, Hoje(tempo), out var dados, out var erros))
                return Results.BadRequest(new ErroResponse("Corrija os campos indicados.", erros));

            var novo = new Atendimento
            {
                Id = Guid.NewGuid(),
                Nome = dados!.Nome,
                DataNascimento = dados.DataNascimento,
                Cpf = dados.Cpf,
                Telefone = dados.Telefone,
                Motivo = dados.Motivo,
                EntradaEm = tempo.GetUtcNow()
            };

            var salvo = await repo.TentarAdicionarAsync(novo);
            if (salvo is null)
            {
                return Results.Conflict(new ErroResponse(
                    "Este CPF já está na fila.",
                    new Dictionary<string, string> { ["cpf"] = "CPF já está na fila." }));
            }

            var posicao = await PosicaoAsync(repo, salvo.Id);
            return Results.Created($"/api/fila/{salvo.Id}", new CadastroResponse(salvo.Id, posicao ?? 1));
        });

        // GET /api/fila/{id}  -> posição atual (tela "Acompanhar minha posição")
        fila.MapGet("/{id:guid}", async (Guid id, IFilaRepository repo) =>
        {
            var item = await repo.ObterAsync(id);
            return item is null
                ? Results.NotFound(new ErroResponse("Cadastro não encontrado."))
                : Results.Ok(await MontarStatusAsync(repo, item));
        });

        // POST /api/fila/{id}/cancelar  -> a pessoa desiste da vaga
        fila.MapPost("/{id:guid}/cancelar", async (Guid id, IFilaRepository repo) =>
        {
            var item = await repo.ObterAsync(id);
            if (item is null)
                return Results.NotFound(new ErroResponse("Cadastro não encontrado."));
            if (item.Status != StatusAtendimento.Aguardando)
                return Results.Conflict(new ErroResponse("Este cadastro não está mais aguardando."));

            var cancelado = await repo.CancelarAsync(id);
            return cancelado is null
                ? Results.Conflict(new ErroResponse("Este cadastro não está mais aguardando."))
                : Results.Ok(await MontarStatusAsync(repo, cancelado));
        });
    }

    // =====================================================================
    // Área da equipe da UPA: protegida por chave (cabeçalho X-Api-Key)
    // =====================================================================
    private static void MapEquipe(RouteGroupBuilder equipe)
    {
        equipe.AddEndpointFilter(async (contexto, proximo) =>
        {
            var config = contexto.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var chave = config["Fila:ChaveAtendimento"];

            // Sem chave configurada, a área da equipe fica desligada (como se não existisse).
            if (string.IsNullOrEmpty(chave))
                return Results.NotFound();

            var enviada = contexto.HttpContext.Request.Headers["X-Api-Key"].ToString();
            var confere = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(enviada), Encoding.UTF8.GetBytes(chave));

            return confere
                ? await proximo(contexto)
                : Results.Json(new ErroResponse("Chave de acesso inválida."), statusCode: StatusCodes.Status401Unauthorized);
        });

        // GET /api/atendimento/fila  -> lista de quem aguarda (CPF mascarado)
        equipe.MapGet("/fila", async (IFilaRepository repo) =>
        {
            var fila = await repo.ListarAguardandoAsync();
            var itens = fila.Select((a, i) => new ItemFilaEquipe(
                i + 1, a.Id, a.Nome, MascararCpf(a.Cpf), a.Telefone, NomeDoMotivo(a.Motivo), a.EntradaEm));
            return Results.Ok(itens);
        });

        // POST /api/atendimento/proximo  -> chama a próxima pessoa
        equipe.MapPost("/proximo", async (IFilaRepository repo, TimeProvider tempo) =>
        {
            var chamado = await repo.ChamarProximoAsync(tempo.GetUtcNow());
            return chamado is null
                ? Results.NotFound(new ErroResponse("Não há ninguém aguardando."))
                : Results.Ok(new PacienteChamado(chamado.Id, chamado.Nome, chamado.Telefone, NomeDoMotivo(chamado.Motivo)));
        });
    }

    // ---------------------------------------------------------------------

    /// <summary>Data de hoje no horário de Brasília (UTC-3), para validar a data de nascimento.</summary>
    private static DateOnly Hoje(TimeProvider tempo) =>
        DateOnly.FromDateTime(tempo.GetUtcNow().UtcDateTime.AddHours(-3));

    /// <summary>Posição (1 = próximo a ser chamado) ou null se não estiver aguardando.</summary>
    private static async Task<int?> PosicaoAsync(IFilaRepository repo, Guid id)
    {
        var fila = await repo.ListarAguardandoAsync();
        var indice = -1;
        for (var i = 0; i < fila.Count; i++)
        {
            if (fila[i].Id == id) { indice = i; break; }
        }
        return indice < 0 ? null : indice + 1;
    }

    private static async Task<StatusResponse> MontarStatusAsync(IFilaRepository repo, Atendimento item)
    {
        var fila = await repo.ListarAguardandoAsync();
        var indice = -1;
        for (var i = 0; i < fila.Count; i++)
        {
            if (fila[i].Id == item.Id) { indice = i; break; }
        }

        return new StatusResponse(
            item.Id,
            item.Status.ToString().ToLowerInvariant(),
            indice < 0 ? null : indice + 1,
            fila.Count,
            item.EntradaEm);
    }

    private static string MascararCpf(string cpf) =>
        cpf.Length == 11 ? $"***.{cpf[3..6]}.{cpf[6..9]}-**" : "***";

    private static string NomeDoMotivo(Motivo motivo) => motivo.ToString().ToLowerInvariant();
}
