//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings
{
    public static class AcessoAlunoMappingExtensions
    {
        public static AcessoAlunoDto ToDto(this AcessoAluno entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var aluno = entity.Aluno;
            var endereco = aluno.Endereco;
            var logradouro = endereco.Logradouro;

            return new AcessoAlunoDto
            {
                Id = entity.Id,
                AlunoId = aluno.Id,
                NomeAluno = aluno.Nome,
                Cpf = aluno.Cpf.Numero,
                DataNascimento = aluno.DataNascimento,
                Telefone = aluno.Telefone.Numero,
                Email = aluno.Email.EnderecoEmail,
                LogradouroId = logradouro.Id,
                Cep = logradouro.Cep.Numero,
                NomeLogradouro = logradouro.Nome,
                Bairro = logradouro.Bairro,
                Cidade = logradouro.Cidade,
                Estado = logradouro.Estado,
                Pais = logradouro.Pais,
                NumeroCasa = endereco.NumeroCasa,
                Complemento = endereco.Complemento,
                Senha = aluno.Senha.TextoPlano,
                Foto = aluno.Foto.Conteudo,
                DataHora = entity.DataHora
            };
        }

        public static AcessoAluno ToEntity(this AcessoAlunoDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var logradouroResult = Logradouro.Criar(dto.LogradouroId, dto.Cep, dto.NomeLogradouro, dto.Bairro, dto.Cidade, dto.Estado, dto.Pais);
            if (logradouroResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(logradouroResult.Notificacoes));

            var cpfResult = Cpf.Criar(dto.Cpf);
            if (cpfResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(cpfResult.Notificacoes));

            var telefoneResult = Telefone.Criar(dto.Telefone);
            if (telefoneResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(telefoneResult.Notificacoes));

            var emailResult = Email.Criar(dto.Email);
            if (emailResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(emailResult.Notificacoes));

            var senhaResult = Senha.Criar(dto.Senha);
            if (senhaResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(senhaResult.Notificacoes));

            var fotoResult = Arquivo.Criar("foto.jpg", dto.Foto);
            if (fotoResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(fotoResult.Notificacoes));

            var alunoResult = Aluno.Criar(
                dto.AlunoId,
                dto.NomeAluno,
                dto.Cpf,
                dto.DataNascimento,
                dto.Telefone,
                dto.Email,
                logradouroResult.Value!,
                dto.NumeroCasa,
                dto.Complemento,
                dto.Senha,
                fotoResult.Value!);

            if (alunoResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(alunoResult.Notificacoes));

            return AcessoAluno.Criar(dto.Id, alunoResult.Value!);
        }

        public static AcessoAluno UpdateFromDto(this AcessoAluno entity, AcessoAlunoDto dto)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(dto);

            return dto.ToEntity();
        }

        private static string FormatErrors(IEnumerable<Notificacoes> notificacoes) =>
            string.Join("; ", notificacoes.Select(n => $"{n.Campo}: {n.Mensagem}"));
    }
}
