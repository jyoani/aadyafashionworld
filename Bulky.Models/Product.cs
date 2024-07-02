using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [Display(Name = "ItemNumber")]
        public string ISBN { get; set; }
        [Required]
        [Display(Name = "Brand")]
        public string Author { get; set; }
        [Required]
        [Display(Name ="Price")]
        [Range(1,1000)]
        public double Price { get; set; }
        
        [Display(Name = "Offer price")]
        [Range(1, 1000)]
        [ValidateNever]
        public double Price50 { get; set; }
        
        [Display(Name = "List Price ")]
        [Range(1, 1000)]
        [ValidateNever]
        public double ListPrice { get; set; }
        
        [Display(Name = "Available quantity")]
        [Range(1, 100)]
        [ValidateNever]
        public double Price100 { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }
        
        
        
        [ValidateNever]
        public string DressColor { get; set; }
        [Required]
        public int Count {  get; set; }


        [ValidateNever]
        public List<ProductImage> ProductImages { get; set; }

    }
}
