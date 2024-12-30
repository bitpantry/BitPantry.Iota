using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;

namespace BitPantry.Iota.Application
{
    public static class PassageDtoExtensions
    {
        public static Card ToCard(this PassageDto passage, long userId, Tab tab, double fractionalOrder)
            => new()
            {
                UserId = userId,
                Address = passage.GetAddressString(),
                BibleId = passage.BibleId,
                StartVerseId = passage.StartVerseId,
                EndVerseId = passage.EndVerseId,
                Tab = tab,
                FractionalOrder = fractionalOrder
            };
    }
}
