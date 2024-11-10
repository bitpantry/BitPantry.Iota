using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Web.Models;

namespace BitPantry.Iota.Web
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
