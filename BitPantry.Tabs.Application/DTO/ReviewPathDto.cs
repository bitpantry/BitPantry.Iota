﻿using BitPantry.Tabs.Common;

namespace BitPantry.Tabs.Application.DTO
{
    public class ReviewPathDto
    {
        public long UserId { get; private set; }
        public Dictionary<Tab, int> Path { get; private set; }

        public ReviewPathDto(long userId, Dictionary<Tab, int> path)
        {
            UserId = userId;
            Path = path;
        }

        public int CardsToReviewCount => Path.Sum(p => p.Value);

        //public Tab? GetNextTab(Tab forTab)
        //{
        //    var keys = Path.Keys.Where(k => k > forTab).OrderBy(k => k).ToList();

        //    if(keys.Any()) 
        //        return keys.FirstOrDefault();

        //    return null;
        //}
            
    }
}
