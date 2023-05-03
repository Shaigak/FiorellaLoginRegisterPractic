using System.ComponentModel.DataAnnotations;

namespace EntityFrameWork.ViewModels.Account
{
    public class LoginVM
    {
        [Required]
        public string EmailOrUserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string PassWord { get; set; }

        public bool IsCheckUp { get; set; }


    }
}
