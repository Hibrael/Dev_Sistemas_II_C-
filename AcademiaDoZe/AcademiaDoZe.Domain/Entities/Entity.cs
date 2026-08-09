//Hibrael Andre Cidade Xavier
namespace AcademiaDoZe.Domain.Entities
{
    /// <summary>
    /// Classe base de toda Entity do domínio: fornece identidade (Id) e igualdade baseada
    /// nessa identidade — diferente de um Value Object, duas Entities são iguais se tiverem
    /// o mesmo Id, mesmo que os demais atributos divirjam.
    /// </summary>
    public abstract class Entity : IEquatable<Entity>
    {
        public int Id { get; protected set; }

        protected Entity(int id = 0)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException(nameof(id), "O identificador da entidade não pode ser negativo.");

            Id = id;
        }

        public bool Equals(Entity? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;

            // Duas entidades ainda não persistidas (Id == 0) nunca são consideradas iguais.
            return Id != 0 && Id == other.Id;
        }

        public override bool Equals(object? obj) => Equals(obj as Entity);

        public override int GetHashCode() => HashCode.Combine(GetType(), Id);
    }
}
