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
    public class OrderDetailRepository : Repository<OrderDetails>, IOrderDetailRepository
    {
        private  ApplicationDbContext db;

        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            this.db = db;
        }

        public void Update(OrderDetails orderDetails)
        {
           // throw new NotImplementedException();
           db.OrderDetails.Update(orderDetails);
        }
    }
}
