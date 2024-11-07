using BitPantry.Iota.Common;
using BitPantry.Iota.Data.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application.DTO
{
    public class CardDto
    {
        public long Id { get; }
        public string Address { get; }
        public DateTime AddedOn { get; }
        public DateTime LastMovedOn { get; }
        public DateTime? LastReviewedOn { get; }
        public Tab Tab { get; }
        public int Order { get; }
        public PassageDto Passage { get; }

        public CardDto(
            long id, 
            string address,
            DateTime addedOn, 
            DateTime lastMovedOn, 
            DateTime? lastReviewedOn, 
            Tab tab, 
            int order,
            List<Verse> verses = null)
        {
            Id = id;
            Address = address;
            AddedOn = addedOn;
            LastMovedOn = lastMovedOn;
            LastReviewedOn = lastReviewedOn;
            Tab = tab;
            Order = order;

            if (verses != null)
                Passage = verses.ToPassageDto();
        }

    }
}