using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class ForgotPassword
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
    }
}
