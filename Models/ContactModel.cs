using Shopping.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping.Models
{
    public class ContactModel
    {
        [Key]
        
        [Required(ErrorMessage = "Yêu cầu nhập trên danh mục ")]
        public string Name { get; set; }  
        
        [Required(ErrorMessage = "Yêu cầu nhập địa chỉ  ")]
        public string Map { get; set; } 
        
        [Required(ErrorMessage = "Yêu cầu nhập Email")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Yêu cầu nhập số điện thoại ")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập thông tin liên hệ")]

        public string Description { get; set; }
        public string LogoImage { get; set; }

        [NotMapped]
        [FileExtention]
        public IFormFile? ImageUpload { get; set; }

    }
}
