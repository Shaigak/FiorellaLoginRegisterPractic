using System.ComponentModel.DataAnnotations;

namespace EntityFrameWork.ViewModels.Account
{
    public class RegisterVM
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress,ErrorMessage ="E-mail is not valid")]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string PassWord { get; set; }
        
        [Required]
        [DataType(DataType.Password),Compare(nameof(PassWord))]
        public string ConfirmPassWord { get; set; }
    }
}
