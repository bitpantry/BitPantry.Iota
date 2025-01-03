using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Tabs.Application.Service
{
    public interface IWorkflowService
    {
        Task DeleteCard(long cardId, CancellationToken cancellationToken);
        Task<ReviewPathDto> GetReviewPath(long userId, DateTime userLocalTime, CancellationToken cancellationToken);
        Task MoveCard(long cardId, Tab toTab, bool toTop, CancellationToken cancellationToken);
        Task PromoteCard(long cardId, CancellationToken cancellationToken);
        Task StartQueueCard(long queueCardId, CancellationToken cancellationToken);
    }
}
