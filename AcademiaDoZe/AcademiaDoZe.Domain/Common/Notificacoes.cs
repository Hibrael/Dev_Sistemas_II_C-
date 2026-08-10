//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Domain.Common
{
    public sealed class Notificacoes
    {
        public string Campo { get; }
        public string Mensagem { get; }

        public Notificacoes(string campo, string mensagem)
        {
            Campo = campo;
            Mensagem = mensagem;
        }

        public override string ToString() => $"{Campo}: {Mensagem}";
    }
}
