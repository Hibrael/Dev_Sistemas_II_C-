//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

/// <summary>
/// Orquestra os casos de uso de Colaborador. Sobre hashing de senha, vale a mesma observação
/// registrada em AlunoService: quem gera salt e hash é o Value Object Senha do domínio.
/// </summary>
public class ColaboradorService : IColaboradorService
{
    private readonly Func<IColaboradorRepository> _repoFactory;

    public ColaboradorService(Func<IColaboradorRepository> repoFactory)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
    }

    public async Task<ColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var colaborador = await _repoFactory().ObterPorId(id, cancellationToken);
        return colaborador?.ToDto();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var colaboradores = await _repoFactory().ObterTodos(cancellationToken);
        return [.. colaboradores.Select(c => c.ToDto())];
    }

    public async Task<ColaboradorDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfVo = CriarCpf(cpf, nameof(cpf));
        var colaborador = await _repoFactory().ObterPorCpf(cpfVo, cancellationToken);
        return colaborador?.ToDto();
    }

    public async Task<ColaboradorDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailVo = CriarEmail(email, nameof(email));
        var colaborador = await _repoFactory().ObterPorEmail(emailVo, cancellationToken);
        return colaborador?.ToDto();
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorTipoAsync(AppColaboradorTipo tipo, CancellationToken cancellationToken = default)
    {
        var colaboradores = await _repoFactory().ObterPorTipo(tipo.ToDomain(), cancellationToken);
        return [.. colaboradores.Select(c => c.ToDto())];
    }

    public async Task<IEnumerable<ColaboradorDto>> ObterPorVinculoAsync(AppColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
    {
        var colaboradores = await _repoFactory().ObterPorVinculo(vinculo.ToDomain(), cancellationToken);
        return [.. colaboradores.Select(c => c.ToDto())];
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

        var colaborador = await _repoFactory().ObterPorId(id, cancellationToken);
        if (colaborador is null) return false;

        return await _repoFactory().TrocarSenha(id, senhaResult.Value!, cancellationToken);
    }

    public async Task<ColaboradorDto> AdicionarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);

        var cpfVo = CriarCpf(colaboradorDto.Cpf, nameof(colaboradorDto));
        var emailVo = CriarEmail(colaboradorDto.Email, nameof(colaboradorDto));
        ValidarSenha(colaboradorDto.Senha, nameof(colaboradorDto));

        if (await _repoFactory().CpfJaExiste(cpfVo, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um colaborador cadastrado com o CPF {colaboradorDto.Cpf}.");

        if (await _repoFactory().EmailJaExiste(emailVo, null, cancellationToken))
            throw new InvalidOperationException($"Já existe um colaborador cadastrado com o email {colaboradorDto.Email}.");

        var colaborador = colaboradorDto.ToEntity();
        var adicionado = await _repoFactory().Adicionar(colaborador, cancellationToken);
        return adicionado.ToDto();
    }

    public async Task<ColaboradorDto> AtualizarAsync(ColaboradorDto colaboradorDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(colaboradorDto);

        var colaboradorExistente = await _repoFactory().ObterPorId(colaboradorDto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Colaborador com ID {colaboradorDto.Id} não encontrado.");

        var cpfVo = CriarCpf(colaboradorDto.Cpf, nameof(colaboradorDto));
        var emailVo = CriarEmail(colaboradorDto.Email, nameof(colaboradorDto));
        ValidarSenha(colaboradorDto.Senha, nameof(colaboradorDto));

        if (await _repoFactory().CpfJaExiste(cpfVo, colaboradorDto.Id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro colaborador cadastrado com o CPF {colaboradorDto.Cpf}.");

        if (await _repoFactory().EmailJaExiste(emailVo, colaboradorDto.Id, cancellationToken))
            throw new InvalidOperationException($"Já existe outro colaborador cadastrado com o email {colaboradorDto.Email}.");

        var colaboradorAtualizado = colaboradorExistente.UpdateFromDto(colaboradorDto);
        var atualizado = await _repoFactory().Atualizar(colaboradorAtualizado, cancellationToken);
        return atualizado.ToDto();
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var colaborador = await _repoFactory().ObterPorId(id, cancellationToken);
        if (colaborador is null) return false;

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
