namespace FilaWeb.Api.Models;

// ---------- Entrada (o que o front-end envia) ----------

/// <summary>
/// Corpo do POST /api/fila. Todos os campos são texto para que a API possa devolver
/// mensagens de erro claras (em vez de um erro genérico de conversão).
/// </summary>
public sealed record CadastroRequest(
    string? Nome,
    string? DataNascimento, // yyyy-MM-dd
    string? Cpf,            // só dígitos (com ou sem máscara, a API limpa)
    string? Telefone,       // só dígitos (com ou sem máscara, a API limpa)
    string? Motivo);        // "sintomas" | "lesao" | "outro"

// ---------- Saída ----------

public sealed record CadastroResponse(Guid Id, int Posicao);

/// <summary>Situação de um cadastro. Não devolve dados pessoais.</summary>
public sealed record StatusResponse(
    Guid Id,
    string Status,        // "aguardando" | "chamado" | "cancelado"
    int? Posicao,         // null quando não está mais aguardando
    int TotalAguardando,
    DateTimeOffset EntradaEm);

public sealed record ErroResponse(string Mensagem, IReadOnlyDictionary<string, string>? Erros = null);

// ---------- Área da equipe (exige chave de acesso) ----------

public sealed record ItemFilaEquipe(
    int Posicao,
    Guid Id,
    string Nome,
    string CpfMascarado,
    string Telefone,
    string Motivo,
    DateTimeOffset EntradaEm);

public sealed record PacienteChamado(Guid Id, string Nome, string Telefone, string Motivo);
