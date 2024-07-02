using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class CouponVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public OrderHeader OrderHeader { get; set; }
        //public double OrderTotal { get; set; }  
        public Cop Coupons { get; set; }
        public IEnumerable<Cop> couponsList { get; set; }
        public int SelectedCouponId { get; set; }
        public double DiscountedAmount { get; set; }
        public IEnumerable<AddressCustomer> AddressList { get; set; }
        public int SelectedAddressId { get; set; }

      //  public int SelectedAddressId { get; set; }
        public double DiscountedTotal { get; set; }
    }
    }
