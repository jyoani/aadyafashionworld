using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class FavListRepository : Repository<Fav>, IFavListRepository
    {
        private  ApplicationDbContext db;

        public FavListRepository(ApplicationDbContext db) : base(db)
        {
            this.db = db;
        }

        void IFavListRepository.Update(Fav item)
        {
           // throw new NotImplementedException();
           db.Favs.Update(item);
        }
    }
}
