using EntityFrameWork.Models;
using System.ComponentModel.DataAnnotations;

namespace EntityFrameWork.Areas.Admin.View_Models
{
    public class ProductUpdateVM
    {

        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Count { get; set; }

        [Required]
        public string Description { get; set; }

        public ICollection<ProductImage> Images { get; set; }

        public List<IFormFile> Photos { get; set; }
        public int CategoryId { get; set; }

    }
}
