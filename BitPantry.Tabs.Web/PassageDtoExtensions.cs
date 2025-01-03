using BitPantry.Tabs.Application.DTO;
using BitPantry.Tabs.Web.Models;

namespace BitPantry.Tabs.Web
{
    public static class PassageDtoExtensions
    {
        public static PassageModel ToModel(this PassageDto dto)
            => new(dto.BibleId,
                    dto.BookName,
                    dto.FromChapterNumber,
                    dto.FromVerseNumber,
                    dto.ToChapterNumber,
                    dto.ToVerseNumber,
                    dto.GetAddressString(),
                    dto.TranslationShortName,
                    dto.Verses);
    }
}
