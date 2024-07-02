using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }
        // public int UserId { get; set; }  // Assuming there's a User model that Wallet is related to.
        public double Balance { get; set; }
        public string User_id { get; set; }

        [ForeignKey("User_id")]
        [ValidateNever]
        public ApplicationUser User { get; set; }

    }
}
