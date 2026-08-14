using System.ComponentModel.DataAnnotations;

namespace net08.Models.ViewModels;

public class NewPostViewModel {
    public int Id { get; set; }
    [Display(Name = "Text")]
    [Required(ErrorMessage = "Povinné pole")]
    public string Text { get; set; }
    [Display(Name = "Image")]
    [Required(ErrorMessage = "Povinné pole")]
    public IFormFile Image { get; set; }
}