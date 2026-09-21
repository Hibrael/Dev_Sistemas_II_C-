//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Application.Security;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

/// <summary>
/// Orquestra os casos de uso de Aluno.
///
/// Sobre hashing de senha: é aqui que a senha vira hash, com Argon2id via PasswordHasher.
/// O Value Object Senha continua validando a força do texto digitado, mas não serve como
/// ponto de hashing para persistência — ele calcula um SHA-256 que ninguém grava, porque os
/// repositórios gravam Senha.TextoPlano. Sem o passo abaixo a coluna senha guardaria o texto
/// puro. Por isso o fluxo é: validar a força do que foi digitado, trocar pelo hash, e só
/// então montar a entidade.
/// </summary>
public class AlunoService : IAlunoService
{
    private readonly Func<IAlunoRepository> _repoFactory;

    public AlunoService(Func<IAlunoRepository> repoFactory)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
    }

    public async Task<AlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Não é preciso buscar o logradouro à parte: o Endereco do domínio já compõe a
        // entidade Logradouro completa, que veio montada pelo repositório.
        var aluno = await _repoFactory().ObterPorId(id, cancellationToken);
        return aluno?.ToDto();
    }

    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var alunos = await _repoFactory().ObterTodos(cancellationToken);
        return [.. alunos.Select(a => a.ToDto())];
    }

    public async Task<AlunoDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfVo = CriarCpf(cpf, nameof(cpf));
        var aluno = await _repoFactory().ObterPorCpf(cpfVo, cancellationToken);
        return aluno?.ToDto();
    }

    public async Task<AlunoDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailVo = CriarEmail(email, nameof(email));
        var aluno = await _repoFactory().ObterPorEmail(emailVo, cancellationToken);
        return aluno?.ToDto();
    }

    public async Task<IEnumerable<AlunoDto>> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio.", nameof(nome));

        // IAlunoRepository não expõe busca por nome; o filtro é feito aqui sobre a listagem
        // para não alterar o contrato do domínio por causa de um caso de uso da aplicação.
        var alunos = await _repoFactory().ObterTodos(cancellationToken);
        var filtro = nome.Trim();

        return [.. alunos
            .Where(a => a.Nome.Contains(filtro, StringComparison.OrdinalIgnoreCase))
            .Select(a => a.ToDto())];
    }

    public async Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;

        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure) return false;

        return await _repoFactory().CpfJaExiste(cpfResult.Value!, id, cancellationToken);
    }

    public async Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure) return false;

        return await _repoFactory().EmailJaExiste(emailResult.Value!, id, cancellationToken);
    }

    public async Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(novaSenha))
            throw new ArgumentException("Nova senha não pode ser vazia.", nameof(novaSenha));

        var senhaResult = Senha.Criar(novaSenha);
        if (senhaResult.IsFailure)
            throw new ArgumentException($"Nova senha inválida: {FormatErrors(senhaResult.Notificacoes)}", nameof(novaSenha));

        var aluno = await _repoFactory().ObterPorId(id, cancellationToken);
        if (aluno is null) return false;

        // Restaurar, e não Criar: o valor já é o hash final e não deve ser revalidado como
        // se fosse uma senha digitada. É o TextoPlano do VO que o repositório grava, então
        // envolver o hash aqui é o que faz a coluna senha receber o Argon2id.
        var senhaHash = Senha.Restaurar(PasswordHasher.Hash(novaSenha));
        return await _repoFactory().TrocarSenha(id, senhaHash, cancellationToken);
    }

    public async Task<AlunoDto> AdicionarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);

        var cpfVo = CriarCpf(alunoDto.Cpf, nameof(alunoDto));
        var emailVo = CriarEmail(alunoDto.Email, nameof(alunoDto));

        // Valida a força do texto digitado e só então o substitui pelo hash, que é o valor
        // que ToEntity leva para a entidade e o repositório grava.
        alunoDto.Senha = PasswordHasher.Hash(ValidarSenha(alunoDto.Senha, nameof(alunoDto)));

        if (await _repoFactory().CpfJaExiste(cpfVo, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um aluno cadastrado com o CPF {alunoDto.Cpf}.");

        if (await _repoFactory().EmailJaExiste(emailVo, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um aluno cadastrado com o email {alunoDto.Email}.");

        var aluno = alunoDto.ToEntity();
        var adicionado = await _repoFactory().Adicionar(aluno, cancellationToken);
        return adicionado.ToDto();
    }

    public async Task<AlunoDto> AtualizarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);

        var alunoExistente = await _repoFactory().ObterPorId(alunoDto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Aluno com ID {alunoDto.Id} não encontrado.");

        var cpfVo = CriarCpf(alunoDto.Cpf, nameof(alunoDto));
        var emailVo = CriarEmail(alunoDto.Email, nameof(alunoDto));

        // Ver comentário equivalente em AdicionarAsync.
        alunoDto.Senha = PasswordHasher.Hash(ValidarSenha(alunoDto.Senha, nameof(alunoDto)));

        if (await _repoFactory().CpfJaExiste(cpfVo, alunoDto.Id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro aluno cadastrado com o CPF {alunoDto.Cpf}.");

        if (await _repoFactory().EmailJaExiste(emailVo, alunoDto.Id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro aluno cadastrado com o email {alunoDto.Email}.");

        var alunoAtualizado = alunoExistente.UpdateFromDto(alunoDto);
        var atualizado = await _repoFactory().Atualizar(alunoAtualizado, cancellationToken);
        return atualizado.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var aluno = await _repoFactory().ObterPorId(id, cancellationToken);
        if (aluno is null) return false;

        return await _repoFactory().Remover(id, cancellationToken);
    }

    private static Cpf CriarCpf(string? cpf, string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF não pode ser vazio.", nomeParametro);

        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure)
            throw new ArgumentException($"CPF inválido: {FormatErrors(cpfResult.Notificacoes)}", nomeParametro);

        return cpfResult.Value!;
    }

    private static Email CriarEmail(string? email, string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio.", nomeParametro);

        var emailResult = Email.Criar(email);
        if (emailResult.IsFailure)
            throw new ArgumentException($"Email inválido: {FormatErrors(emailResult.Notificacoes)}", nomeParametro);

        return emailResult.Value!;
    }

    /// <summary>Valida a força da senha digitada e a devolve, pronta para ser hasheada.</summary>
    private static string ValidarSenha(string? senha, string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("Senha não pode ser vazia.", nomeParametro);

        var senhaResult = Senha.Criar(senha);
        if (senhaResult.IsFailure)
            throw new ArgumentException($"Senha não atende aos requisitos mínimos: {FormatErrors(senhaResult.Notificacoes)}", nomeParametro);

        return senha;
    }

    private static string FormatErrors(IEnumerable<Notificacoes> notificacoes) =>
        string.Join(", ", notificacoes.Select(n => n.Mensagem));
}
