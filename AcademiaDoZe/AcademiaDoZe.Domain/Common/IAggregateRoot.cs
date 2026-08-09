namespace AcademiaDoZe.Domain.Common
{
    /// <summary>
    /// Marca uma entidade como raiz de agregado no domínio.
    /// Uma raiz de agregado é a porta de entrada para um conjunto de objetos relacionados
    /// que devem ser tratados como uma única unidade de consistência.
    /// </summary>
    public interface IAggregateRoot
    {
    }
}
