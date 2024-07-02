using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext db;

        public UserRepository(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<IdentityUser>> GetAll()
        {
           // throw new NotImplementedException();
           var users=await db.Users.ToListAsync();
            var adminuser=await db.Users.FirstOrDefaultAsync(x => x.Email == "Admin@gmail.com");   
            if(adminuser != null) { 
            users.Remove(adminuser);
            }
            return users;
        }
    }
}
