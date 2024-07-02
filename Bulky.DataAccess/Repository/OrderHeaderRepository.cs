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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private  ApplicationDbContext db;

        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            this.db = db;
        }

        public void Update(OrderHeader orderHeader)
        {
           // throw new NotImplementedException();
           db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderstatus, string paymentstatus = null)
        {
            var orderFromDb = db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderstatus;
                if (!string.IsNullOrEmpty(paymentstatus))
                {
                    orderFromDb.PaymentStatus = paymentstatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            //   throw new NotImplementedException();
            var orderFromDb = db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDb.PaymentIntentId = paymentIntentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
