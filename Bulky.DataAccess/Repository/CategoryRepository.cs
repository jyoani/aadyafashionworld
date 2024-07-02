using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class categoryRepository : Repository<Category>, IcategoryRepository
    {
        private readonly ApplicationDbContext db;

        public categoryRepository(ApplicationDbContext db) : base(db)
        {
            this.db = db;
        }

        //public void Save()
        //{
        //    db.SaveChanges();
        //}

        public void Update(Category category)
        {
            db.categories.Update(category); 
        }
    }
}
