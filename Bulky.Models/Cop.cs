using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class Cop
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Coupon name is required")]
        public string CouponName { get; set; }

        [Range(0, 50, ErrorMessage = "Percentage must be between 0 and 50")]
        public int Percentage { get; set; }
        [ValidateNever]
        [Range(0, double.MaxValue, ErrorMessage = "Discount rate must be a positive number")]
        public double DiscountRate { get; set; }

        public DateTime CouponGeneratedDate { get; set; } // Use DateTime for storing date and time

        public DateTime ExpiryDate { get; set; }

        public bool IsExpired => DateTime.Now > ExpiryDate;

        public Cop()
        {
            CouponGeneratedDate = DateTime.Now;
        }

    }

}
