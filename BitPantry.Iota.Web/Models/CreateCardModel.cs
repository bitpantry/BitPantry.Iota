using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework.Profiler;
using System.ComponentModel.DataAnnotations;

namespace BitPantry.Iota.Web.Models
{
    public class CreateCardModel
    {
        [Required(ErrorMessage = "Passage is required.")]
        [Display(Name = "Passage")]
        public string Passage { get; set; }

        public List<SelectListItem> Translations { get; set; }

        [Required(ErrorMessage = "Translation is required.")]
        [Display(Name = "Translation")]
        public string SelectedTranslation { get; set; }

        public CreateCardModel() { }

        public CreateCardModel(List<SelectListItem> bibleTranslationsSelectList)
        {
            Translations = bibleTranslationsSelectList;
        }

    }
}
