using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Web.Models;

namespace BitPantry.Iota.Web
{
    public static class CardDtoExtensions
    {
        public static CardModel ToModel(this CardDto dto)
            => new (dto.Id,
                    dto.Address,
                    dto.AddedOn,
                    dto.LastMovedOn,
                    dto.LastReviewedOn,
                    dto.Tab,
                    dto.Order,
                    dto.Passage?.ToModel());
    }
}
