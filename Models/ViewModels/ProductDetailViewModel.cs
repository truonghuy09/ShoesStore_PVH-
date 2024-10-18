using System.ComponentModel.DataAnnotations;

namespace Shopping.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public ProductModel ProductDetails { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập bình luận sản phẩm")]
        public string Comment { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên")]

        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Email")]

        public string Email { get; set; }
    }
}
