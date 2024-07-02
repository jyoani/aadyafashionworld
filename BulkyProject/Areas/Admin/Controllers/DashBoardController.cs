using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyProject.Areas.Admin.Controllers
{
     [Area("Admin")]
        [Authorize]
        public class DashBoardController : Controller
        {
            private readonly IUnitOfWork _unitOfWork;

            [BindProperty]
            public DashBoardVM dashboardVm { get; set; }

            public DashBoardController(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork;
            }
            [Authorize(Roles = SD.Role_Admin)]
            public IActionResult Index(int page = 1, int itemsPerPage = 5)
            {
                IEnumerable<OrderHeader> orderHeaders = _unitOfWork.orderHeaderRepository.GetAll(includeProperties: "ApplicationUser");
                IEnumerable<Product> productList = _unitOfWork.product.GetAll();
                IEnumerable<Category> catogoryList = _unitOfWork.category.GetAll();

                int shippedCount = orderHeaders.Count(u => u.OrderStatus == "Shipped");
                int approvedCount = orderHeaders.Count(u => u.OrderStatus == "Approved");
                int cancelledCount = orderHeaders.Count(u => u.OrderStatus == "Cancelled");
                int productCount = productList.Count();
                int orderCount = orderHeaders.Count();
                int categoryCount = catogoryList.Count();
                double totalSales = orderHeaders.Sum(order => order.orderTotal);

                DateTime today = DateTime.Now;
                DateTime lastWeek = today.AddDays(-7);
                IEnumerable<OrderHeader> ordersLastWeek = orderHeaders.Where(order => order.OrderDate >= lastWeek && order.OrderDate <= today)
                                                                      .OrderByDescending(order => order.OrderDate);
                int numberOfOrdersLastWeek = ordersLastWeek.Count();
                double totalRevenueLastWeek = ordersLastWeek.Sum(order => order.orderTotal);

                DateTime lastWeek1 = today.AddDays(-7);
                DateTime lastWeek2 = today.AddDays(-14);
                DateTime lastWeek3 = today.AddDays(-21);

                IEnumerable<OrderHeader> ordersWeek1 = orderHeaders.Where(order => order.OrderDate >= today && order.OrderDate <= lastWeek1)
                                                                   .OrderByDescending(order => order.OrderDate);
                IEnumerable<OrderHeader> ordersWeek2 = orderHeaders.Where(order => order.OrderDate >= lastWeek1 && order.OrderDate < lastWeek2)
                                                                   .OrderByDescending(order => order.OrderDate);
                IEnumerable<OrderHeader> ordersWeek3 = orderHeaders.Where(order => order.OrderDate >= lastWeek2 && order.OrderDate < lastWeek3)
                                                                   .OrderByDescending(order => order.OrderDate);

                double totalRevenueWeek1 = ordersWeek1.Sum(order => order.orderTotal);
                double totalRevenueWeek2 = ordersWeek2.Sum(order => order.orderTotal);
                double totalRevenueWeek3 = ordersWeek3.Sum(order => order.orderTotal);

                var chartData = new List<double> { totalRevenueWeek1, totalRevenueWeek2, totalRevenueWeek3 };
                var chartLabels = new List<string> { "Week 1", "Week 2", "Week 3" };

                var topSellingProducts = _unitOfWork.orderDetailRepository.GetAll(includeProperties: "Product")
                    .GroupBy(od => new { od.ProductId, od.Product.Title })
                    .Select(g => new ProductSalesVM
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Title,
                        TotalSold = g.Sum(od => od.Count)
                    })
                    .OrderByDescending(p => p.TotalSold)
                    .Take(10)
                    .ToList();

                var viewModel = new DashBoardVM
                {
                    Categories = catogoryList,
                    Products = productList,
                    OrderHeaders = ordersLastWeek.Skip((page - 1) * itemsPerPage).Take(itemsPerPage),
                    OrderCount = orderCount,
                    ProductCount = productCount,
                    CategoryCount = categoryCount,
                    TotalSales = totalSales,
                    ApprovedCount = approvedCount,
                    CancelledCount = cancelledCount,
                    ShippedCount = shippedCount,
                    TotalRevenueLastWeek = totalRevenueLastWeek,
                    NumberOfOrdersLastWeek = numberOfOrdersLastWeek,
                    CurrentPage = page,
                    ItemsPerPage = itemsPerPage,
                    TopSellingProducts = topSellingProducts // Add the top selling products to the view model
                };

                return View(viewModel);
            }

            [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_User}")]

            public IActionResult Invoice()
            {
                IEnumerable<OrderHeader> orderHeaders = _unitOfWork.orderHeaderRepository.GetAll(includeProperties: "ApplicationUser");
                DateTime today = DateTime.Now;
                DateTime lastWeek = today.AddDays(-7);
                IEnumerable<OrderHeader> ordersLastWeek = orderHeaders.Where(order => order.OrderDate >= lastWeek && order.OrderDate <= today)
                                                                      .OrderByDescending(order => order.OrderDate);
                double totalRevenueLastWeek = ordersLastWeek.Sum(order => order.orderTotal);
                int cancelledCount = ordersLastWeek.Count(u => u.OrderStatus == "Cancelled");
                int orderCount = ordersLastWeek.Count();

                var viewModel = new DashBoardVM
                {
                    OrderHeaders = ordersLastWeek,
                    TotalRevenueLastWeek = totalRevenueLastWeek,
                    CancelledCount = cancelledCount,
                    OrderCount = orderCount
                };

                return View(viewModel);
            }

            [HttpPost]
            public JsonResult Index()
            {
                try
                {
                    IEnumerable<OrderHeader> orderHeaders = _unitOfWork.orderHeaderRepository.GetAll(includeProperties: "ApplicationUser");

                    DateTime today = DateTime.Now;
                    DateTime lastWeek = today.AddDays(-7);
                    IEnumerable<OrderHeader> ordersLastWeek = orderHeaders.Where(order => order.OrderDate >= lastWeek && order.OrderDate <= today)
                                                                          .OrderByDescending(order => order.OrderDate);
                    int numberOfOrdersLastWeek = ordersLastWeek.Count();
                    double totalRevenueLastWeek = ordersLastWeek.Sum(order => order.orderTotal);

                    DateTime lastWeek1 = today.AddDays(-7);
                    DateTime lastWeek2 = today.AddDays(-14);
                    DateTime lastWeek3 = today.AddDays(-21);

                    IEnumerable<OrderHeader> ordersWeek1 = orderHeaders.Where(order => order.OrderDate >= lastWeek1 && order.OrderDate <= today)
                                                                       .OrderByDescending(order => order.OrderDate);
                    IEnumerable<OrderHeader> ordersWeek2 = orderHeaders.Where(order => order.OrderDate >= lastWeek2 && order.OrderDate < lastWeek1)
                                                                       .OrderByDescending(order => order.OrderDate);
                    IEnumerable<OrderHeader> ordersWeek3 = orderHeaders.Where(order => order.OrderDate >= lastWeek1 && order.OrderDate < lastWeek2)
                                                                       .OrderByDescending(order => order.OrderDate);

                    double totalRevenueWeek1 = ordersWeek1.Sum(order => order.orderTotal);
                    double totalRevenueWeek2 = ordersWeek2.Sum(order => order.orderTotal);
                    double totalRevenueWeek3 = ordersLastWeek.Sum(order => order.orderTotal);

                    var chartData = new List<double> { totalRevenueWeek1, totalRevenueWeek2, totalRevenueWeek3 };
                    var chartLabels = new List<string> { "Week 1", "Week 2", "Week 3" };

                    return Json(new { ChartLabels = chartLabels, ChartData = chartData });
                }
                catch (Exception ex)
                {
                    return Json(new { error = "An error occurred while processing the request." });
                }
            }

        }
    }
