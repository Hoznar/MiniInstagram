using System.ComponentModel.DataAnnotations;

namespace net08.Models.ViewModels;

public class PasswordViewModel {
    [Required(ErrorMessage = "Povinné pole")]
    [DataType(DataType.Password)]
    [Display(Name = "Staré heslo")]
    public string OldPassword { get; set; }
    
    [Required(ErrorMessage = "Povinné pole")]
    [DataType(DataType.Password)]
    [Display(Name = "Nové heslo")]
    public string NewPassword { get; set; }
    
    [Required(ErrorMessage = "Povinné pole")]
    [DataType(DataType.Password)]
    [Display(Name = "Podtrvdit nové Heslo")]
    [Compare("NewPassword", ErrorMessage = "Hesla se neshodují")]
    public string ConfirmPassword { get; set; }
}