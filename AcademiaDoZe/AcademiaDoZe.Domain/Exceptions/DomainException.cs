//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Domain.Exceptions
{

    public sealed class DomainException : Exception
    {
        public DomainException(string mensagem) : base(mensagem)
        {
        }

        public DomainException(string mensagem, Exception innerException) : base(mensagem, innerException)
        {
        }
    }
}
