//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Domain.Common
{
    /// <summary>
    /// Representa uma violação de regra de negócio (invariante) encontrada durante a
    /// construção/validação de uma Entity ou Value Object.
    ///
    /// Em vez de lançar exceções a cada campo inválido (o que interrompe a validação no
    /// primeiro erro), os métodos de fábrica estáticos "Criar" acumulam uma lista de
    /// Notification e, ao final, retornam tudo de uma vez através de Result{T}.Failure.
    /// </summary>
    public sealed class Notification
    {
        /// <summary>Nome do campo/propriedade que originou a notificação (ex.: "Nome").</summary>
        public string Campo { get; }

        /// <summary>Código/mensagem da regra violada (ex.: "NOME_OBRIGATORIO").</summary>
        public string Mensagem { get; }

        public Notification(string campo, string mensagem)
        {
            Campo = campo;
            Mensagem = mensagem;
        }

        public override string ToString() => $"{Campo}: {Mensagem}";
    }
}
