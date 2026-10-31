using System.Text.Json;
using System.Text.Json.Serialization;
using FilaWeb.Api.Models;

namespace FilaWeb.Api.Services;

/// <summary>
/// Guarda a fila em memória e grava tudo num arquivo JSON a cada mudança, para os dados
/// sobreviverem a um reinício do servidor. Serve para desenvolvimento e demonstração:
/// funciona com um único servidor rodando. Para produção, use um banco de dados.
/// </summary>
public sealed class JsonFilaRepository : IFilaRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly object _lock = new();
    private readonly string? _caminho;
    private readonly List<Atendimento> _itens;

    /// <param name="caminhoArquivo">Arquivo onde gravar. Null = só em memória (nada é salvo).</param>
    public JsonFilaRepository(string? caminhoArquivo)
    {
        _caminho = caminhoArquivo;
        _itens = Carregar();
    }

    public Task<Atendimento?> TentarAdicionarAsync(Atendimento novo)
    {
        lock (_lock)
        {
            var jaNaFila = _itens.Any(a => a.Status == StatusAtendimento.Aguardando && a.Cpf == novo.Cpf);
            if (jaNaFila)
                return Task.FromResult<Atendimento?>(null);

            _itens.Add(novo);
            Salvar();
            return Task.FromResult<Atendimento?>(novo);
        }
    }

    public Task<Atendimento?> ObterAsync(Guid id)
    {
        lock (_lock)
            return Task.FromResult(_itens.FirstOrDefault(a => a.Id == id));
    }

    public Task<IReadOnlyList<Atendimento>> ListarAguardandoAsync()
    {
        lock (_lock)
        {
            IReadOnlyList<Atendimento> fila = _itens
                .Where(a => a.Status == StatusAtendimento.Aguardando)
                .OrderBy(a => a.EntradaEm)
                .ToList();
            return Task.FromResult(fila);
        }
    }

    public Task<Atendimento?> ChamarProximoAsync(DateTimeOffset agora)
    {
        lock (_lock)
        {
            var proximo = _itens
                .Where(a => a.Status == StatusAtendimento.Aguardando)
                .OrderBy(a => a.EntradaEm)
                .FirstOrDefault();
            if (proximo is null)
                return Task.FromResult<Atendimento?>(null);

            var chamado = proximo with { Status = StatusAtendimento.Chamado, ChamadoEm = agora };
            Substituir(chamado);
            return Task.FromResult<Atendimento?>(chamado);
        }
    }

    public Task<Atendimento?> CancelarAsync(Guid id)
    {
        lock (_lock)
        {
            var atual = _itens.FirstOrDefault(a => a.Id == id);
            if (atual is null || atual.Status != StatusAtendimento.Aguardando)
                return Task.FromResult<Atendimento?>(null);

            var cancelado = atual with { Status = StatusAtendimento.Cancelado };
            Substituir(cancelado);
            return Task.FromResult<Atendimento?>(cancelado);
        }
    }

    // ---- internos (sempre chamados dentro do lock) ----

    private void Substituir(Atendimento novo)
    {
        var indice = _itens.FindIndex(a => a.Id == novo.Id);
        _itens[indice] = novo;
        Salvar();
    }

    private List<Atendimento> Carregar()
    {
        if (_caminho is null || !File.Exists(_caminho))
            return [];

        var json = File.ReadAllText(_caminho);
        return JsonSerializer.Deserialize<List<Atendimento>>(json, JsonOptions) ?? [];
    }

    private void Salvar()
    {
        if (_caminho is null)
            return;

        var pasta = Path.GetDirectoryName(_caminho);
        if (!string.IsNullOrEmpty(pasta))
            Directory.CreateDirectory(pasta);

        // Grava num arquivo temporário e troca, para não deixar o arquivo pela metade se der erro.
        var temporario = _caminho + ".tmp";
        File.WriteAllText(temporario, JsonSerializer.Serialize(_itens, JsonOptions));
        File.Move(temporario, _caminho, overwrite: true);
    }
}
