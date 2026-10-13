//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using System.Text.RegularExpressions;

namespace AcademiaDoZe.Presentation.AppMaui.Helpers;

/// <summary>
/// Validações comuns a Colaborador e Aluno (dados de PessoaDto), no padrão Notification Pattern:
/// cada regra acrescenta uma mensagem à lista, e a tela mostra todas de uma vez.
/// As regras espelham os Value Objects do domínio (Cpf, Telefone, Email, Senha, Arquivo), para que o
/// usuário veja os erros aqui, antes de o Service lançar exceção apenas na primeira falha.
/// </summary>
public static partial class PessoaValidacao
{
    /// <summary>Mesmo limite do Value Object Arquivo.</summary>
    public const long FotoTamanhoMaximoBytes = 5 * 1024 * 1024;

    /// <summary>Mesmas extensões aceitas pelo Value Object Arquivo.</summary>
    public static readonly string[] FotoExtensoesPermitidas = [".jpg", ".jpeg", ".png"];

    public static string ApenasDigitos(string? texto) => new([.. (texto ?? string.Empty).Where(char.IsDigit)]);

    /// <summary>CPF com 11 dígitos, não repetidos, e dígitos verificadores corretos (igual ao VO Cpf).</summary>
    public static bool CpfValido(string? cpf)
    {
        var digitos = ApenasDigitos(cpf);
        if (digitos.Length != 11 || new string(digitos[0], 11) == digitos)
            return false;

        return CalcularDigitoVerificador(digitos, 9) == digitos[9] - '0'
            && CalcularDigitoVerificador(digitos, 10) == digitos[10] - '0';
    }

    private static int CalcularDigitoVerificador(string cpf, int tamanho)
    {
        var soma = 0;
        for (var i = 0; i < tamanho; i++)
            soma += (cpf[i] - '0') * (tamanho + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    /// <summary>
    /// Mesma regra do VO Email: sem espaços, nome sem ponto, e domínio com um único ponto (ex.: nome@dominio.com).
    /// </summary>
    public static bool EmailValido(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var limpo = EspacosRegex().Replace(email, string.Empty).ToLowerInvariant();
        return FormatoEmailRegex().IsMatch(limpo);
    }

    /// <summary>Mesma regra do VO Senha: no mínimo 8 caracteres, com letras e números.</summary>
    public static bool SenhaValida(string? senha)
        => !string.IsNullOrWhiteSpace(senha)
           && senha.Length >= 8
           && senha.Any(char.IsLetter)
           && senha.Any(char.IsDigit);

    /// <summary>Extensão e tamanho aceitos pelo VO Arquivo.</summary>
    public static bool FotoExtensaoValida(string? nomeArquivo)
        => FotoExtensoesPermitidas.Contains(Path.GetExtension(nomeArquivo ?? string.Empty).ToLowerInvariant());

    /// <summary>
    /// Valida os dados de pessoa compartilhados por Colaborador e Aluno.
    /// A senha é exigida também na edição: o Service sempre regrava a senha ao atualizar.
    /// </summary>
    public static void ValidarDadosPessoais(PessoaDto pessoa, string confirmarSenha, int idadeMinima, string rotulo, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(pessoa.Nome))
            errors.Add("• Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(pessoa.Cpf))
            errors.Add("• CPF é obrigatório.");
        else if (ApenasDigitos(pessoa.Cpf).Length != 11)
            errors.Add("• CPF deve conter exatamente 11 dígitos numéricos.");
        else if (!CpfValido(pessoa.Cpf))
            errors.Add("• CPF inválido (dígitos verificadores não conferem).");

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        if (pessoa.DataNascimento == default)
            errors.Add("• Data de nascimento é obrigatória.");
        else if (pessoa.DataNascimento > hoje)
            errors.Add("• Data de nascimento não pode ser futura.");
        else if (pessoa.DataNascimento > hoje.AddYears(-idadeMinima))
            errors.Add($"• O {rotulo} deve ter no mínimo {idadeMinima} anos de idade.");

        if (string.IsNullOrWhiteSpace(pessoa.Telefone))
            errors.Add("• Telefone é obrigatório.");
        else if (ApenasDigitos(pessoa.Telefone).Length is not (10 or 11))
            errors.Add("• Telefone deve conter 10 ou 11 dígitos numéricos, com DDD.");

        if (string.IsNullOrWhiteSpace(pessoa.Email))
            errors.Add("• E-mail é obrigatório.");
        else if (!EmailValido(pessoa.Email))
            errors.Add("• E-mail inválido. Use o formato nome@dominio.com (sem ponto antes do @ e com um único ponto no domínio).");

        if (string.IsNullOrWhiteSpace(pessoa.Senha))
            errors.Add("• Senha é obrigatória (na edição, informe a senha atual ou uma nova).");
        else
        {
            if (!SenhaValida(pessoa.Senha.Trim()))
                errors.Add("• A senha deve ter no mínimo 8 caracteres, com letras e números.");
            if (pessoa.Senha != confirmarSenha)
                errors.Add("• A confirmação de senha não confere com a senha informada.");
        }

        if (pessoa.Foto is null || pessoa.Foto.Conteudo is not { Length: > 0 })
            errors.Add("• Foto é obrigatória. Use \"Alterar foto\" para escolher da galeria ou tirar com a câmera.");
        else if (pessoa.Foto.Conteudo.LongLength > FotoTamanhoMaximoBytes)
            errors.Add("• A foto deve ter no máximo 5 MB.");

        if (pessoa.Endereco is null || string.IsNullOrWhiteSpace(pessoa.Endereco.Cep))
            errors.Add("• CEP do endereço é obrigatório.");

        if (string.IsNullOrWhiteSpace(pessoa.Numero))
            errors.Add("• Número do endereço é obrigatório.");
    }

    /// <summary>Normaliza os dados antes de persistir (mesmo tratamento do material).</summary>
    public static void Normalizar(PessoaDto pessoa)
    {
        pessoa.Nome = pessoa.Nome.Trim();
        pessoa.Cpf = ApenasDigitos(pessoa.Cpf);
        pessoa.Telefone = ApenasDigitos(pessoa.Telefone);
        pessoa.Numero = pessoa.Numero.Trim();
        pessoa.Complemento = string.IsNullOrWhiteSpace(pessoa.Complemento) ? null : pessoa.Complemento.Trim();
        pessoa.Email = string.IsNullOrWhiteSpace(pessoa.Email) ? null : pessoa.Email.Trim();
        pessoa.Senha = string.IsNullOrWhiteSpace(pessoa.Senha) ? null : pessoa.Senha.Trim();
    }

    /// <summary>Mostra todos os erros num único alerta. Retorna true quando não há erros.</summary>
    public static async Task<bool> ExibirErrosAsync(List<string> errors)
    {
        if (errors.Count == 0)
            return true;

        var mensagem = "Por favor, corrija os seguintes campos:\n\n" + string.Join("\n", errors);
        await Shell.Current.DisplayAlertAsync("Erros de Validação", mensagem, "OK");
        return false;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacosRegex();

    // cópia literal da regex do VO Email
    [GeneratedRegex(@"^[^@.]+@[^@.]+.[^@.]+$")]
    private static partial Regex FormatoEmailRegex();
}
