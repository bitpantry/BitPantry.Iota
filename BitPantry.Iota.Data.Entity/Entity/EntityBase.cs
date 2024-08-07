using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitPantry.Iota.Data.Entity
{
    /// <summary>
    /// Base class for entities that must have an identifier attribute. If Identity Type is Guid, a new one is auto generated here.
    /// </summary>
    /// <typeparam name="TIdentityType">The type of the identifier attribute for the entity</typeparam>
    [Serializable]
    public abstract class EntityBase<TIdentityType> :
            IEntityBase<TIdentityType>, IEquatable<EntityBase<TIdentityType>>
    {
        [NotMapped]
        public object ObjectId { get; private set; }

        [Key]
        [Column(Order = 1)]
        public virtual TIdentityType Id
        {
            get
            {
                if (ObjectId == null && typeof(TIdentityType) == typeof(Guid))
                    ObjectId = Guid.NewGuid();

                if (ObjectId == null)
                    return default(TIdentityType);

                return (TIdentityType)ObjectId;
            }

            set { ObjectId = value; }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = 17;
                result = result * 23 + (ObjectId?.GetHashCode() ?? 0);
                return result;
            }
        }

        [NotMapped]
        public bool IsNew => Id.Equals(default(TIdentityType));

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(EntityBase<TIdentityType>)) return false;

            return Equals((EntityBase<TIdentityType>)obj);
        }

        public bool Equals(EntityBase<TIdentityType> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;

            return other.Id.Equals(Id);
        }

        public static bool operator ==(EntityBase<TIdentityType> left, EntityBase<TIdentityType> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityBase<TIdentityType> left, EntityBase<TIdentityType> right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"{GetType().FullName} :: {Id}";
        }
    }
}
