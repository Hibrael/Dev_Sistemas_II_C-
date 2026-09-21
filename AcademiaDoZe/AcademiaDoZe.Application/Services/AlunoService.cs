//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

/// <summary>
/// Orquestra os casos de uso de Aluno.
///
/// Sobre hashing de senha: o material de aula aplica Argon2id aqui, na camada de aplicação,
/// porque no exemplo dele o Value Object Senha apenas valida a força. Neste projeto o Senha
/// do domínio já gera salt e hash dentro de Criar, então hashear de novo aqui produziria um
/// hash sobre outro hash e o Senha.Verificar nunca mais conferiria. Por isso o hashing fica
/// num ponto único — o domínio — e este serviço só valida e repassa o texto digitado.
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

        // Senha.Criar já devolve o Value Object com salt e hash gerados.
        return await _repoFactory().TrocarSenha(id, senhaResult.Value!, cancellationToken);
    }

    public async Task<AlunoDto> AdicionarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(alunoDto);

        var cpfVo = CriarCpf(alunoDto.Cpf, nameof(alunoDto));
        var emailVo = CriarEmail(alunoDto.Email, nameof(alunoDto));
        ValidarSenha(alunoDto.Senha, nameof(alunoDto));

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
        ValidarSenha(alunoDto.Senha, nameof(alunoDto));

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

    private static void ValidarSenha(string? senha, string nomeParametro)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new ArgumentException("Senha não pode ser vazia.", nomeParametro);

        var senhaResult = Senha.Criar(senha);
        if (senhaResult.IsFailure)
            throw new ArgumentException($"Senha não atende aos requisitos mínimos: {FormatErrors(senhaResult.Notificacoes)}", nomeParametro);
    }

    private static string FormatErrors(IEnumerable<Notificacoes> notificacoes) =>
        string.Join(", ", notificacoes.Select(n => n.Mensagem));
}
