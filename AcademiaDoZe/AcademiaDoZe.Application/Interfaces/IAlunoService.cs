//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

/// <summary>
/// Contrato de serviço para operações de negócios relacionadas a Alunos.
/// </summary>
public interface IAlunoService
{
    /// <summary>Obtém um aluno por ID, incluindo o endereço mapeado quando disponível.</summary>
    Task<AlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Obtém a listagem completa de alunos cadastrados.</summary>
    Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default);

    /// <summary>Cadastra um novo aluno após validação de CPF, email e senha.</summary>
    Task<AlunoDto> AdicionarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default);

    /// <summary>Atualiza os dados de um aluno existente, preservando a integridade dos dados.</summary>
    Task<AlunoDto> AtualizarAsync(AlunoDto alunoDto, CancellationToken cancellationToken = default);

    /// <summary>Remove o cadastro de um aluno pelo ID.</summary>
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Obtém um aluno buscando pelo seu CPF.</summary>
    Task<AlunoDto?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);

    /// <summary>Obtém um aluno buscando pelo seu email.</summary>
    Task<AlunoDto?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Obtém alunos filtrando pelo nome.</summary>
    Task<IEnumerable<AlunoDto>> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);

    /// <summary>Verifica se já existe um aluno com o CPF informado.</summary>
    Task<bool> CpfJaExisteAsync(string cpf, int? id = null, CancellationToken cancellationToken = default);

    /// <summary>Verifica se já existe um aluno com o email informado.</summary>
    Task<bool> EmailJaExisteAsync(string email, int? id = null, CancellationToken cancellationToken = default);

    /// <summary>Realiza a troca de senha do aluno, gerando um novo hash pelo Value Object Senha.</summary>
    Task<bool> TrocarSenhaAsync(int id, string novaSenha, CancellationToken cancellationToken = default);
}
