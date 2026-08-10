//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um arquivo binário armazenado pelo domínio (usado como
    /// foto de Aluno/Colaborador). Guarda o conteúdo em memória junto com metadados
    /// mínimos para validação e exibição.
    ///
    /// ATENÇÃO (suposição): a lista de extensões permitidas e o tamanho máximo abaixo são um
    /// valor razoável escolhido por mim para tornar a classe funcional (o material fornecido
    /// não especifica essas regras) — ajuste as constantes conforme a regra de negócio real
    /// da Academia do Zé para a foto de cadastro.
    /// </summary>
    public sealed record Arquivo
    {
        private static readonly string[] ExtensoesPermitidas = [".jpg", ".jpeg", ".png"];
        private const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB

        public string Nome { get; }
        public string Extensao { get; }
        public long TamanhoBytes { get; }
        public byte[] Conteudo { get; }

        private Arquivo(string nome, string extensao, long tamanhoBytes, byte[] conteudo)
        {
            Nome = nome;
            Extensao = extensao;
            TamanhoBytes = tamanhoBytes;
            Conteudo = conteudo;
        }

        public static Result<Arquivo> Criar(string nome, byte[] conteudo)
        {
            var notificacoes = new List<Notificacoes>();

            if (NormalizadoService.TextoVazioOuNulo(nome))
                notificacoes.Add(new Notificacoes("Nome", "ARQUIVO_NOME_OBRIGATORIO"));

            if (conteudo is null || conteudo.Length == 0)
                notificacoes.Add(new Notificacoes("Conteudo", "ARQUIVO_CONTEUDO_OBRIGATORIO"));

            if (notificacoes.Count != 0)
                return Result<Arquivo>.Failure(notificacoes);

            var extensao = NormalizadoService.ParaMinusculo(Path.GetExtension(nome));

            if (!ExtensoesPermitidas.Contains(extensao))
                notificacoes.Add(new Notificacoes("Extensao", "ARQUIVO_EXTENSAO_INVALIDA"));

            if (conteudo!.LongLength > TamanhoMaximoBytes)
                notificacoes.Add(new Notificacoes("Conteudo", "ARQUIVO_TAMANHO_EXCEDIDO"));

            if (notificacoes.Count != 0)
                return Result<Arquivo>.Failure(notificacoes);

            return Result<Arquivo>.Success(new Arquivo(NormalizadoService.LimparEspacos(nome), extensao, conteudo.LongLength, conteudo));
        }
    }
}



