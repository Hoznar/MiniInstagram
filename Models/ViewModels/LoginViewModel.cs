using System.ComponentModel.DataAnnotations;

namespace net08.Models.ViewModels;

public class LoginViewModel {
    [Required(ErrorMessage = "Povinné pole")]
    [EmailAddress(ErrorMessage = "Nevalidní email adresa")]
    [Display(Name = "Email")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Povinné pole")]
    [DataType(DataType.Password)]
    [Display(Name = "Heslo")]
    public string Password { get; set; }
}