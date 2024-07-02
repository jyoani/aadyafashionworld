using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models
{
    public class AddressCustomer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phonenumber { get; set; }
        public string Area { get; set; }
        public string BuildingName { get; set; }


        public string  Postalcode { get; set; }

        public string State { get; set; }
       
        public string User_id { get; set; }
        [ForeignKey("User_id")]
        [ValidateNever]
        public ApplicationUser User { get; set; }

    }
}
