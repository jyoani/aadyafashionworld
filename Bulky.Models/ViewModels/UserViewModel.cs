using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class UserViewModel
    {
        public List<User> Users { get; set; }       
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool Admincheck { get; set; }
        public bool IsNotBlocked { get; set; }=true;
       
    }
}
