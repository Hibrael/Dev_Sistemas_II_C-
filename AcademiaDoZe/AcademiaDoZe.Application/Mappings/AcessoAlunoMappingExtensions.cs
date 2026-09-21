//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings
{
    public static class AcessoAlunoMappingExtensions
    {
        private const string FotoNomePadrao = "foto.jpg";

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
                // A senha não acompanha a saída: ver AcessoAlunoDto.Senha.
                Senha = null,
                Foto = aluno.Foto.Conteudo,
                FotoNomeArquivo = aluno.Foto.Nome,
                DataHora = entity.DataHora
            };
        }

        public static AcessoAluno ToEntity(this AcessoAlunoDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var senha = dto.Senha;
            if (string.IsNullOrWhiteSpace(senha))
                throw new InvalidOperationException("Senha: SENHA_OBRIGATORIA");

            var logradouroResult = Logradouro.Criar(dto.LogradouroId, dto.Cep, dto.NomeLogradouro, dto.Bairro, dto.Cidade, dto.Estado, dto.Pais);
            if (logradouroResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(logradouroResult.Notificacoes));

            var fotoResult = Arquivo.Criar(dto.FotoNomeArquivo ?? FotoNomePadrao, dto.Foto);
            if (fotoResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(fotoResult.Notificacoes));

            // Cpf, Telefone, Email e Senha não são construídos aqui de propósito: Aluno.Criar
            // já cria cada um desses Value Objects e agrega TODAS as notificações numa lista
            // só. Pré-validar duplicaria a regra e ainda reportaria apenas a primeira falha.
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
                senha,
                fotoResult.Value!);

            if (alunoResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(alunoResult.Notificacoes));

            // Sem DataHora o registro é um check-in novo e quem carimba o instante é o domínio;
            // com DataHora preenchida o instante informado é preservado. Essa distinção importa
            // no AtualizarAsync, que senão reescreveria o horário real do acesso com "agora".
            return dto.DataHora == default
                ? AcessoAluno.Criar(dto.Id, alunoResult.Value!)
                : AcessoAluno.Restaurar(dto.Id, alunoResult.Value!, dto.DataHora);
        }

        private static string FormatErrors(IEnumerable<Notificacoes> notificacoes) =>
            string.Join("; ", notificacoes.Select(n => $"{n.Campo}: {n.Mensagem}"));
    }
}
