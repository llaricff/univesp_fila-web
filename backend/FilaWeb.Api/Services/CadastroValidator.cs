using System.Globalization;
using FilaWeb.Api.Models;

namespace FilaWeb.Api.Services;

/// <summary>Dados já validados e limpos, prontos para virar um Atendimento.</summary>
public sealed record DadosCadastro(
    string Nome,
    DateOnly DataNascimento,
    string Cpf,
    string Telefone,
    Motivo Motivo);

/// <summary>
/// Regras de validação do cadastro. Espelham as do front-end (src/utils/validators.js),
/// mas o back-end é quem manda: nunca confie só na validação do navegador.
/// As chaves do dicionário de erros são os nomes dos campos da tela:
/// nome, nascimento, cpf, telefone, motivo.
/// </summary>
public static class CadastroValidator
{
    public static bool TryValidar(
        CadastroRequest? req,
        DateOnly hoje,
        out DadosCadastro? dados,
        out Dictionary<string, string> erros)
    {
        req ??= new CadastroRequest(null, null, null, null, null);
        erros = new Dictionary<string, string>();
        dados = null;

        // Nome: pelo menos nome e sobrenome. Espaços repetidos viram um só.
        var partes = (req.Nome ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var nome = string.Join(' ', partes);
        if (partes.Length < 2 || nome.Length > 120)
            erros["nome"] = "Informe nome e sobrenome.";

        // Data de nascimento: yyyy-MM-dd, não pode ser futura nem ter mais de 120 anos.
        if (!DateOnly.TryParseExact(req.DataNascimento, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var nascimento)
            || nascimento > hoje
            || nascimento < hoje.AddYears(-120))
        {
            erros["nascimento"] = "Informe uma data de nascimento válida.";
        }

        var cpf = SomenteDigitos(req.Cpf);
        if (!CpfValido(cpf))
            erros["cpf"] = "CPF inválido.";

        var telefone = SomenteDigitos(req.Telefone);
        if (telefone.Length is not (10 or 11))
            erros["telefone"] = "Informe DDD e número.";

        Motivo? motivo = req.Motivo?.Trim().ToLowerInvariant() switch
        {
            "sintomas" => Motivo.Sintomas,
            "lesao" => Motivo.Lesao,
            "outro" => Motivo.Outro,
            _ => null
        };
        if (motivo is null)
            erros["motivo"] = "Escolha o motivo da visita.";

        if (erros.Count > 0)
            return false;

        dados = new DadosCadastro(nome, nascimento, cpf, telefone, motivo!.Value);
        return true;
    }

    public static string SomenteDigitos(string? valor) =>
        valor is null ? "" : new string(valor.Where(c => c is >= '0' and <= '9').ToArray());

    /// <summary>Confere os dois dígitos verificadores do CPF.</summary>
    public static bool CpfValido(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
            return false;

        int Digito(int tamanho)
        {
            var soma = 0;
            for (var i = 0; i < tamanho; i++)
                soma += (cpf[i] - '0') * (tamanho + 1 - i);
            var resto = soma * 10 % 11;
            return resto == 10 ? 0 : resto;
        }

        return Digito(9) == cpf[9] - '0' && Digito(10) == cpf[10] - '0';
    }
}
