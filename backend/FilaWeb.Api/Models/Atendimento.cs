namespace FilaWeb.Api.Models;

public enum Motivo
{
    Sintomas,
    Lesao,
    Outro
}

public enum StatusAtendimento
{
    Aguardando,
    Chamado,
    Cancelado
}

/// <summary>
/// Uma pessoa na fila de triagem. Imutável: para mudar o status, cria-se uma cópia com "with".
/// </summary>
public sealed record Atendimento
{
    public required Guid Id { get; init; }
    public required string Nome { get; init; }
    public required DateOnly DataNascimento { get; init; }

    /// <summary>Somente dígitos (11).</summary>
    public required string Cpf { get; init; }

    /// <summary>Somente dígitos (DDD + número).</summary>
    public required string Telefone { get; init; }

    public required Motivo Motivo { get; init; }
    public required DateTimeOffset EntradaEm { get; init; }
    public StatusAtendimento Status { get; init; } = StatusAtendimento.Aguardando;
    public DateTimeOffset? ChamadoEm { get; init; }
}
