using BitPantry.Iota.Application.DTO;
using BitPantry.Iota.Web.Models;

namespace BitPantry.Iota.Web
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
