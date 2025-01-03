using System.ComponentModel.DataAnnotations.Schema;

namespace BitPantry.Tabs.Data.Entity
{
    public interface IEntityBase<TIdentityType> : IEntityBase
    {
        TIdentityType Id { get; set; }
    }

    /// <summary>
    /// Marker interface for classes in the domain model that are entities and not value objects
    /// </summary>
    public interface IEntityBase
    {
        [NotMapped]
        bool IsNew { get; }

        [NotMapped]
        object ObjectId { get; }
    }
}
