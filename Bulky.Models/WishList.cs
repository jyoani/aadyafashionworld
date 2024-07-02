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
    public class WishList
    {
        [Key]
        public int Id { get; set; }

        public string ProductNameFav { get; set; }
        public string ImageUrlFav { get; set; }

        [Required]
        public int Product_id { get; set; }
        [ForeignKey("Product_id")]
        [ValidateNever]
        public Product Product { get; set; }

        public string User_id { get; set; }
        [ForeignKey("User_id")]
        [ValidateNever]
        public ApplicationUser User { get; set; }
        [ValidateNever]
        public string description { get; set; } 
    }
}
