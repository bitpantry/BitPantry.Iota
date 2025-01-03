using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Web.Models;

namespace BitPantry.Tabs.Web
{
    public static class BibleDtoExtensions
    {
        public static BibleModel ToModel(this BibleDto dto)
        {
            return new BibleModel(
                dto.Id,
                dto.LongName,
                dto.ShortName);
        }
    }
}
