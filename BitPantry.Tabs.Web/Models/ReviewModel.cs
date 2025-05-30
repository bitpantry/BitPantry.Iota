﻿using BitPantry.Tabs.Common;

namespace BitPantry.Tabs.Web.Models
{
    public record ReviewModel(
        Dictionary<Tab, int> Path, 
        Tab CurrentTab, 
        int CurrentOrder, 
        CardModel Card, 
        string NextUrl, 
        WorkflowType WorkflowType,
        int QueueCardCount) 
    {

        public bool EnablePromote 
        {
            get
            {
                if (WorkflowType == WorkflowType.Basic && CurrentTab == Tab.Daily)
                    return true;

                if (WorkflowType == WorkflowType.Advanced && CurrentTab < Tab.Day1)
                    return true;

                return false;
            }
        }

    }
}
