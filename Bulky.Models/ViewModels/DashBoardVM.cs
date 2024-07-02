using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class DashBoardVM
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<OrderHeader> OrderHeaders { get; set; }

        public int OrderCount { get; set; }
        public int ProductCount { get; set; }
        public int CategoryCount { get; set; }
        public int ApprovedCount { get; set; }
        public int CancelledCount { get; set; }
        public int ShippedCount { get; set; }
        public double TotalSales { get; set; }
        public double TotalRevenueLastWeek { get; set; }
        public int NumberOfOrdersLastWeek { get; set; }

        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }


        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<OrderDetails> OrderDetail { get; set; }
        public List<string> ChartLabels { get; set; }
        public List<double> ChartData { get; set; }


        public List<ProductSalesVM> TopSellingProducts { get; set; }
    }

}
