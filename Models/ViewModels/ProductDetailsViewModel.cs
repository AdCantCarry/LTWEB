using System.Collections.Generic;
using TechNova.Models.Core;

namespace TechNova.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<ProductReview> Reviews { get; set; }
        public bool CanReview { get; set; }
        public ProductReview NewReview { get; set; }
    }
}
