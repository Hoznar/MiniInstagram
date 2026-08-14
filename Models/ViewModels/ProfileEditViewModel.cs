using System.ComponentModel.DataAnnotations;

namespace net08.Models.ViewModels;

public class ProfileEditViewModel {
    public int? Id { get; set; }
    [Display(Name = "Avatar")]
    public IFormFile? Avatar { get; set; }
    
    [Required(ErrorMessage = "Povinné pole")]
    [Display(Name = "Jméno")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Povinné pole")]
    [Display(Name = "Přijmění")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "Povinné pole")]
    [Display(Name = "Věk")]
    public int Age { get; set; }
    
    [Required(ErrorMessage = "Povinné pole")]
    [Display(Name = "Uživatelské jméno")]
    public string Username { get; set; }
}