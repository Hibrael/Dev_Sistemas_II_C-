//Hibrael Andre Cidade Xavier
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities
{
    /// <summary>
    /// Logradouro é uma Entity (possui Id/identidade própria) e não um Value Object porque,
    /// na prática, o mesmo logradouro é compartilhado por vários endereços de alunos e
    /// colaboradores diferentes — faz sentido cadastrá-lo e reutilizá-lo por identidade,
    /// em vez de duplicar seus dados a cada Endereco criado.
    /// </summary>
    public sealed class Logradouro : Entity, IAggregateRoot
    {
        public Cep Cep { get; private set; }
        public string Nome { get; private set; }
        public string Bairro { get; private set; }
        public string Cidade { get; private set; }
        public string Estado { get; private set; }
        public string Pais { get; private set; }

        private Logradouro(int id, Cep cep, string nome, string bairro, string cidade, string estado, string pais) : base(id)
        {
            Cep = cep;
            Nome = nome;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            Pais = pais;
        }

        public static Result<Logradouro> Criar(int id, string cep, string nome, string bairro, string cidade, string estado, string pais)
        {
            var notificacoes = new List<Notification>();

            var cepResult = Cep.Criar(cep);
            if (cepResult.IsFailure)
                notificacoes.AddRange(cepResult.Notifications);

            if (NormalizadoService.TextoVazioOuNulo(nome))
                notificacoes.Add(new Notification("Nome", "NOME_OBRIGATORIO"));
            else
                nome = NormalizadoService.LimparEspacos(nome);

            if (NormalizadoService.TextoVazioOuNulo(bairro))
                notificacoes.Add(new Notification("Bairro", "BAIRRO_OBRIGATORIO"));
            else
                bairro = NormalizadoService.LimparEspacos(bairro);

            if (NormalizadoService.TextoVazioOuNulo(cidade))
                notificacoes.Add(new Notification("Cidade", "CIDADE_OBRIGATORIO"));
            else
                cidade = NormalizadoService.LimparEspacos(cidade);

            if (NormalizadoService.TextoVazioOuNulo(estado))
                notificacoes.Add(new Notification("Estado", "ESTADO_OBRIGATORIO"));
            else
                estado = NormalizadoService.ParaMaiusculo(NormalizadoService.LimparTodosEspacos(estado));

            if (NormalizadoService.TextoVazioOuNulo(pais))
                notificacoes.Add(new Notification("Pais", "PAIS_OBRIGATORIO"));
            else
                pais = NormalizadoService.LimparEspacos(pais);

            if (notificacoes.Count != 0)
                return Result<Logradouro>.Failure(notificacoes);

            return Result<Logradouro>.Success(new Logradouro(id, cepResult.Value!, nome, bairro, cidade, estado, pais));
        }
    }
}



