using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.Service
{
    public class AdvancedWorkflowService : IWorkflowService
    {
        public Task DeleteCard(long cardId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ReviewPathDto> GetReviewPath(long userId, DateTime userLocalTime, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task MoveCard(long cardId, Tab toTab, bool toTop, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task PromoteCard(long cardId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SwapTopQueueCardForDaily(long queueCardId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
