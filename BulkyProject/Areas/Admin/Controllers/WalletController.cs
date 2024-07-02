using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BulkyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IUnitOfWork unitOfWork;

        public OrderVM OrderVM { get; set; }
        public WalletController(ApplicationDbContext context,IUnitOfWork unitOfWork)
        {
            this.context = context;
            this.unitOfWork = unitOfWork;
            // _unitOfWork = unitOfWork;
        }




      
        [Authorize]
        public IActionResult Index()
        {
            try
            {
                // Get the current user's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Retrieve the user's wallet
                var wallet = context.wallets.FirstOrDefault(w => w.User_id == userId);

                
                // Pass the wallet balance to the view
                ViewBag.WalletBalance = wallet.Balance;

                return View();
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        private Wallet CreateNewWallet(string userId)
        {
            return new Wallet
            {
                User_id = userId,
                Balance = 0 // Initialize with a zero balance or any other default value
            };
        }

        public IActionResult Indexcoreext()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Retrieve or create the wallet for the user
                var wallet = context.wallets.FirstOrDefault(w => w.User_id == userId);
                if (wallet == null)
                {
                    wallet = CreateNewWallet(userId);
                    context.wallets.Add(wallet); // Mark wallet as added to context
                    context.SaveChanges(); // Save changes to ensure wallet gets an ID
                }

                // Retrieve orders with status "Cancelled" for the user
                var ordersWithCancelStatus = context.OrderHeaders
                    .Where(o => o.ApplicationUserId == userId && o.OrderStatus == SD.StatusCancelled)
                    .ToList();

                // Process each cancelled order and update wallet balance
                foreach (var orderHeader in ordersWithCancelStatus)
                {
                    // Retrieve order details (assuming orderHeaderId is available)
                    var orderDetails = unitOfWork.orderDetailRepository.Get(u => u.OrderHeaderId == orderHeader.Id);

                    if (orderDetails != null)
                    {
                        // Calculate refunded amount based on order total
                        int refundedAmount = (int)orderHeader.orderTotal;

                        // Update wallet balance
                        wallet.Balance += refundedAmount;
                        ViewBag.Balance=wallet.Balance;
                        // Optionally, update order status or perform other actions
                        orderHeader.OrderStatus = SD.StatusCancelled;
                    }
                }

                // Save all changes at once to ensure transaction integrity
                context.SaveChanges();

                return View(wallet);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                // For example, log the error and return an error view/message
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

       
        public IActionResult Index1()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Retrieve or create the wallet for the user
                var wallet = context.wallets.FirstOrDefault(w => w.User_id == userId);
                if (wallet == null)
                {
                    wallet = CreateNewWallet(userId);
                    context.wallets.Add(wallet); // Mark wallet as added to context
                }

                // Retrieve orders with status "Cancelled" for the user
                var ordersWithCancelStatus = context.OrderHeaders
                    .Where(o => o.ApplicationUserId == userId && o.OrderStatus == SD.StatusCancelled)
                    .ToList();

                // Process each cancelled order and update wallet balance
                foreach (var orderHeader in ordersWithCancelStatus)
                {
                    // Retrieve order details (assuming orderHeaderId is available)
                    var orderDetails = unitOfWork.orderDetailRepository.Get(u => u.OrderHeaderId == orderHeader.Id);

                    if (orderDetails != null)
                    {
                        // Calculate refunded amount based on order total
                        int refundedAmount = (int)orderHeader.orderTotal;

                        // Update wallet balance
                        wallet.Balance += refundedAmount;

                        // Mark wallet as modified to update in database
                        context.Entry(wallet).State = EntityState.Modified;

                        // Optionally, update order status or perform other actions
                        orderHeader.OrderStatus = SD.StatusCancelled;
                    }
                }

                // Save changes to database (update wallet balance and order statuses)
                context.SaveChanges();

                // Retrieve updated balance after processing orders
                var bal = wallet.Balance;

                // Pass the wallet balance to the view using ViewBag
                ViewBag.Balance = bal;

                // Return the view
                return View();
            }
            catch (DbUpdateException ex)
            {
                // Log the details of the inner exception
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    Console.WriteLine(innerException.Message);
                    innerException = innerException.InnerException;
                }

                // Handle or rethrow the exception as needed
                throw;
            }
        }

        public IActionResult Index111()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
           
            var wallet = context.wallets.FirstOrDefault(w => w.User_id == userId);

            var orderHeader = unitOfWork.orderHeaderRepository.Get(u => u.Id == OrderVM.OrderHeader.Id);
            var orderDetails = unitOfWork.orderDetailRepository.Get(u => u.Id == OrderVM.OrderHeader.Id);

            if (orderHeader.PaymentStatus == SD.StatusRefunded)
            {
                wallet.Balance = (double)orderDetails.Price;

                //wallet.Balance = orderDetails.Price;
            }

            return View(wallet.Balance);

           
        }

        public IActionResult AddFunds(double amount)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

          //  int userId = 1; // This should come from user authentication context
            var wallet = context.wallets.FirstOrDefault(w => w.User_id == userId);
            if (wallet == null)
            {
                return View("Error", new ErrorViewModel { RequestId = "No wallet found" });
            }

            wallet.Balance += amount;
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult WithdrawFunds(double amount)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

          //  userId = 1; // This should come from user authentication context
            var wallet = context.wallets.FirstOrDefault(w => w.User_id == userId);
            if (wallet == null || wallet.Balance < amount)
            {
                return View("Error", new ErrorViewModel { RequestId = "Insufficient funds or no wallet found" });
            }

            wallet.Balance -= amount;
            context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
