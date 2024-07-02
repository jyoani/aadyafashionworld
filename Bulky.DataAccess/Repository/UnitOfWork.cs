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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public IcategoryRepository category { get; private set; }

        public IProductRepository product { get; private set; }   
        

       // public IShoppingCartRepository shoppingCart { get; private set; }
       public IShoppingCartRepository shoppingCartRepository { get; private set; }

        public IApplicationUserRepository applicationUser { get; private set; }

        public IOrderHeaderRepository orderHeaderRepository {  get; private set; }  
        public IOrderDetailRepository orderDetailRepository {  get; private set; }  

        public IFavListRepository favListRepository { get; private set; }
        public IProductImageRepository productImageRepository { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            category = new categoryRepository(_db);
            product = new ProductRepository(_db);
            shoppingCartRepository = new ShoppingCartRepository(_db);
            applicationUser = new ApplicationUserRepository(_db);
            orderHeaderRepository=new OrderHeaderRepository(_db);
            orderDetailRepository = new OrderDetailRepository(_db);
            favListRepository=new FavListRepository(_db);
            productImageRepository=new ProductImageRepository(_db); 
        }


        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
