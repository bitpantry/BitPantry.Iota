using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Common;
using BitPantry.Tabs.Data.Entity;

namespace BitPantry.Tabs.Application
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
