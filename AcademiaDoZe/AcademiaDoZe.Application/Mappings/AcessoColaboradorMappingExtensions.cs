//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Mappings
{
    public static class AcessoColaboradorMappingExtensions
    {
        public static AcessoColaboradorDto ToDto(this AcessoColaborador entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var colaborador = entity.Colaborador;
            var endereco = colaborador.Endereco;
            var logradouro = endereco.Logradouro;

            return new AcessoColaboradorDto
            {
                Id = entity.Id,
                ColaboradorId = colaborador.Id,
                NomeColaborador = colaborador.Nome,
                Cpf = colaborador.Cpf.Numero,
                DataNascimento = colaborador.DataNascimento,
                Telefone = colaborador.Telefone.Numero,
                Email = colaborador.Email.EnderecoEmail,
                LogradouroId = logradouro.Id,
                Cep = logradouro.Cep.Numero,
                NomeLogradouro = logradouro.Nome,
                Bairro = logradouro.Bairro,
                Cidade = logradouro.Cidade,
                Estado = logradouro.Estado,
                Pais = logradouro.Pais,
                NumeroCasa = endereco.NumeroCasa,
                Complemento = endereco.Complemento,
                Senha = colaborador.Senha.TextoPlano,
                Foto = colaborador.Foto.Conteudo,
                DataAdmissao = colaborador.DataAdmissao,
                Tipo = colaborador.Tipo.ToString(),
                Vinculo = colaborador.Vinculo.ToString(),
                Salario = colaborador.Salario,
                DataHora = entity.DataHora
            };
        }

        public static AcessoColaborador ToEntity(this AcessoColaboradorDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var logradouroResult = Logradouro.Criar(dto.LogradouroId, dto.Cep, dto.NomeLogradouro, dto.Bairro, dto.Cidade, dto.Estado, dto.Pais);
            if (logradouroResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(logradouroResult.Notificacoes));

            if (!Enum.TryParse<ColaboradorTipo>(dto.Tipo, true, out var tipo))
                throw new InvalidOperationException($"Tipo: TIPO_COLABORADOR_INVALIDO");

            if (!Enum.TryParse<ColaboradorVinculo>(dto.Vinculo, true, out var vinculo))
                throw new InvalidOperationException($"Vinculo: VINCULO_COLABORADOR_INVALIDO");

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

            var colaboradorResult = Colaborador.Criar(
                dto.ColaboradorId,
                dto.NomeColaborador,
                dto.Cpf,
                dto.DataNascimento,
                dto.Telefone,
                dto.Email,
                logradouroResult.Value!,
                dto.NumeroCasa,
                dto.Complemento,
                dto.Senha,
                fotoResult.Value!,
                dto.DataAdmissao,
                tipo,
                vinculo,
                dto.Salario);

            if (colaboradorResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(colaboradorResult.Notificacoes));

            return AcessoColaborador.Criar(dto.Id, colaboradorResult.Value!);
        }

        public static AcessoColaborador UpdateFromDto(this AcessoColaborador entity, AcessoColaboradorDto dto)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(dto);

            return dto.ToEntity();
        }

        private static string FormatErrors(IEnumerable<Notificacoes> notificacoes) =>
            string.Join("; ", notificacoes.Select(n => $"{n.Campo}: {n.Mensagem}"));
    }
}
