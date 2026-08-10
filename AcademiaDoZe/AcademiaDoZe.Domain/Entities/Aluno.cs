//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities
{
    public sealed class Aluno : Pessoa, IAggregateRoot
    {
        private Aluno(int id, string nome, Cpf cpf, DateOnly dataNascimento, Telefone telefone, Email email,
            Endereco endereco, Senha senha, Arquivo foto)
            : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
        {
        }

        public static Result<Aluno> Criar(int id, string nome, string cpf, DateOnly dataNascimento,
            string telefone, string email, Logradouro endereco, string numeroCasa, string? complemento,
            string senha, Arquivo foto)
        {
            var notificacoes = new List<Notificacoes>();

            if (NormalizadoService.TextoVazioOuNulo(nome))
                notificacoes.Add(new Notificacoes("Nome", "NOME_OBRIGATORIO"));
            else
                nome = NormalizadoService.LimparEspacos(nome);

            if (dataNascimento == default)
                notificacoes.Add(new Notificacoes("DataNascimento", "DATA_NASCIMENTO_OBRIGATORIO"));
            else if (dataNascimento > DateOnly.FromDateTime(DateTime.Today.AddYears(-14)))

                notificacoes.Add(new Notificacoes("DataNascimento", "DATA_NASCIMENTO_MINIMA_INVALIDA"));

            if (foto is null)
                notificacoes.Add(new Notificacoes("Foto", "FOTO_OBRIGATORIA"));

            var cpfResult = Cpf.Criar(cpf);
            if (cpfResult.IsFailure) notificacoes.AddRange(cpfResult.Notificacoes);

            var telefoneResult = Telefone.Criar(telefone);
            if (telefoneResult.IsFailure) notificacoes.AddRange(telefoneResult.Notificacoes);

            var emailResult = Email.Criar(email);
            if (emailResult.IsFailure) notificacoes.AddRange(emailResult.Notificacoes);

            var senhaResult = Senha.Criar(senha);
            if (senhaResult.IsFailure) notificacoes.AddRange(senhaResult.Notificacoes);

            var enderecoResult = Endereco.Criar(endereco, numeroCasa, complemento);
            if (enderecoResult.IsFailure) notificacoes.AddRange(enderecoResult.Notificacoes);

            if (notificacoes.Count != 0)
                return Result<Aluno>.Failure(notificacoes);

            var aluno = new Aluno(id, nome, cpfResult.Value!, dataNascimento, telefoneResult.Value!,
                emailResult.Value!, enderecoResult.Value!, senhaResult.Value!, foto!);

            return Result<Aluno>.Success(aluno);
        }
    }
}



