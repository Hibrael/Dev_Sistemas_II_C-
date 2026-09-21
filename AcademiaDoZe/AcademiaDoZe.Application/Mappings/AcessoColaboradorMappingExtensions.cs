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
        private const string FotoNomePadrao = "foto.jpg";

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
                // A senha não acompanha a saída: ver AcessoColaboradorDto.Senha.
                Senha = null,
                Foto = colaborador.Foto.Conteudo,
                FotoNomeArquivo = colaborador.Foto.Nome,
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

            var senha = dto.Senha;
            if (string.IsNullOrWhiteSpace(senha))
                throw new InvalidOperationException("Senha: SENHA_OBRIGATORIA");

            var logradouroResult = Logradouro.Criar(dto.LogradouroId, dto.Cep, dto.NomeLogradouro, dto.Bairro, dto.Cidade, dto.Estado, dto.Pais);
            if (logradouroResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(logradouroResult.Notificacoes));

            if (!Enum.TryParse<ColaboradorTipo>(dto.Tipo, true, out var tipo))
                throw new InvalidOperationException($"Tipo: TIPO_COLABORADOR_INVALIDO ({dto.Tipo})");

            if (!Enum.TryParse<ColaboradorVinculo>(dto.Vinculo, true, out var vinculo))
                throw new InvalidOperationException($"Vinculo: VINCULO_COLABORADOR_INVALIDO ({dto.Vinculo})");

            var fotoResult = Arquivo.Criar(dto.FotoNomeArquivo ?? FotoNomePadrao, dto.Foto);
            if (fotoResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(fotoResult.Notificacoes));

            // Cpf, Telefone, Email e Senha não são construídos aqui de propósito: Colaborador.Criar
            // já cria cada um desses Value Objects e agrega TODAS as notificações numa lista
            // só. Pré-validar duplicaria a regra e ainda reportaria apenas a primeira falha.
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
                senha,
                fotoResult.Value!,
                dto.DataAdmissao,
                tipo,
                vinculo,
                dto.Salario);

            if (colaboradorResult.IsFailure)
                throw new InvalidOperationException(FormatErrors(colaboradorResult.Notificacoes));

            // Sem DataHora o registro é um check-in novo e quem carimba o instante é o domínio;
            // com DataHora preenchida o instante informado é preservado. Essa distinção importa
            // no AtualizarAsync, que senão reescreveria o horário real do acesso com "agora".
            return dto.DataHora == default
                ? AcessoColaborador.Criar(dto.Id, colaboradorResult.Value!)
                : AcessoColaborador.Restaurar(dto.Id, colaboradorResult.Value!, dto.DataHora);
        }

        private static string FormatErrors(IEnumerable<Notificacoes> notificacoes) =>
            string.Join("; ", notificacoes.Select(n => $"{n.Campo}: {n.Mensagem}"));
    }
}
