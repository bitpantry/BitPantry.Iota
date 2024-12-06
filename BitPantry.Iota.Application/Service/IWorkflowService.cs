using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public interface IWorkflowService
    {
        Task DeleteCard(long cardId, CancellationToken cancellationToken);
        Task<ReviewPathDto> GetReviewPath(long userId, DateTime userLocalTime, CancellationToken cancellationToken);
        Task MoveCard(long cardId, Tab toTab, bool toTop, CancellationToken cancellationToken);
        Task PromoteCard(long cardId, CancellationToken cancellationToken);
        Task SwapTopQueueCardForDaily(long queueCardId, CancellationToken cancellationToken);
    }
}
