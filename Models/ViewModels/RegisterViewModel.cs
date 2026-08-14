using System.ComponentModel.DataAnnotations;

namespace net08.Models.ViewModels;

public class RegisterViewModel {
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
    
    [Required(ErrorMessage = "Povinné pole")]
    [EmailAddress(ErrorMessage = "Nevalidní email adresa")]
    [Display(Name = "Email")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Povinné pole")]
    [DataType(DataType.Password)]
    [Display(Name = "Heslo")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Povinné pole")]
    [DataType(DataType.Password)]
    [Display(Name = "Podtrvdit Heslo")]
    [Compare("Password", ErrorMessage = "Hesla se neshodují")]
    public string ConfirmPassword { get; set; }
}