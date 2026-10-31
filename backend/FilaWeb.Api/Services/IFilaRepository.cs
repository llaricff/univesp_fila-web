using FilaWeb.Api.Models;

namespace FilaWeb.Api.Services;

/// <summary>
/// Onde a fila é guardada. Hoje existe só a versão em arquivo JSON (JsonFilaRepository).
/// Para usar um banco de dados de verdade (SQL Server, PostgreSQL...), crie outra classe que
/// implemente esta interface e troque o registro em Program.cs. Os endpoints não mudam.
/// Os métodos são assíncronos de propósito, porque acesso a banco é assíncrono.
/// </summary>
public interface IFilaRepository
{
    /// <summary>
    /// Adiciona ao fim da fila. Retorna null, sem adicionar, se o mesmo CPF já está aguardando
    /// (checagem e inclusão acontecem juntas, para duas requisições simultâneas não duplicarem).
    /// </summary>
    Task<Atendimento?> TentarAdicionarAsync(Atendimento novo);

    Task<Atendimento?> ObterAsync(Guid id);

    /// <summary>Quem está aguardando, em ordem de chegada.</summary>
    Task<IReadOnlyList<Atendimento>> ListarAguardandoAsync();

    /// <summary>Chama o primeiro da fila (muda para "chamado"). Retorna null se a fila está vazia.</summary>
    Task<Atendimento?> ChamarProximoAsync(DateTimeOffset agora);

    /// <summary>Cancela um cadastro que está aguardando. Retorna null se não existir ou não estiver aguardando.</summary>
    Task<Atendimento?> CancelarAsync(Guid id);
}
