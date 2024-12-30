using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Common;
using BitPantry.Iota.Web.Models;
using Humanizer;

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
                    dto.ReviewCount,
                    dto.RowNumber,
                    dto.Passage?.ToModel());

        public static CardMaintenanceModel ToMaintenanceModel(this CardDto dto, WorkflowType workflowType)
            => new CardMaintenanceModel(dto.ToModel(), workflowType);
    }
}
