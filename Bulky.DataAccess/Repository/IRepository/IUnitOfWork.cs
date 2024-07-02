using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IcategoryRepository category { get; }

        IProductRepository product { get; }

       // IShoppingCartRepository shoppingCart { get; }
        IShoppingCartRepository shoppingCartRepository { get; }
        IFavListRepository favListRepository { get; }

        IProductImageRepository productImageRepository { get; }

        IApplicationUserRepository applicationUser { get; }
        IOrderHeaderRepository orderHeaderRepository { get; }
        IOrderDetailRepository orderDetailRepository { get; }
        void Save();
    }
}
