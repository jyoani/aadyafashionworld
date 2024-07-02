using Bulky.DataAccess.Data;
using Bulky.DataAccess.Migrations;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork unitofwork;
        private readonly ApplicationDbContext db;

        [BindProperty]
        public CouponVM ShoppingCartVM { get; set; }
       

        public CartController(IUnitOfWork unitofwork,ApplicationDbContext db)
        {
            this.unitofwork = unitofwork;
            this.db = db;
        }

        public IActionResult Discount(int id)
        {
            // Get user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get shopping cart items for the user
            var shoppingCartItems = unitofwork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            // Get the selected coupon by its ID
            var selectedCoupon = db.cops.FirstOrDefault(c => c.Id == id);

            // Calculate the discount amount based on the selected coupon
            double discountAmount = 0;
            if (selectedCoupon != null)
            {
                foreach (var cartItem in shoppingCartItems)
                {
                    cartItem.Price = GetPrice(cartItem); // Ensure to get the correct price for each cart item

                    if (selectedCoupon.Percentage > 0)
                    {
                        discountAmount += (cartItem.Price * selectedCoupon.Percentage) / 100;
                    }
                    else if (selectedCoupon.DiscountRate > 0)
                    {
                        discountAmount += selectedCoupon.DiscountRate;
                    }
                }
            }
            double orderTotal = 0;
            foreach (var cartItem in shoppingCartItems)
            {
                orderTotal += (cartItem.Price * cartItem.Count);
            }

            // Calculate order total with discounts
            double discountedTotal = orderTotal - discountAmount;

            // Create ShoppingCartVM instance
            var shoppingCartVM = new CouponVM
            {
                ShoppingCartList = shoppingCartItems,
                SelectedCouponId = id, // Set the selected coupon ID
                DiscountedAmount = discountAmount, // Pass the calculated discount amount to the view
                OrderHeader = new OrderHeader(),
                DiscountedTotal = discountedTotal,
                AddressList = db.addresses.ToList().Where(u => u.User_id == userId),
            };

            // Calculate order total with discounts
            foreach (var cartItem in shoppingCartVM.ShoppingCartList)
            {
                shoppingCartVM.OrderHeader.orderTotal += (cartItem.Price * cartItem.Count);
                shoppingCartVM.OrderHeader.orderTotal = shoppingCartVM.OrderHeader.orderTotal - shoppingCartVM.DiscountedAmount;
                shoppingCartVM.OrderHeader.orderDiscount = shoppingCartVM.DiscountedAmount;
            }
            // Assign the calculated discount amount to a property in the view model
            shoppingCartVM.DiscountedAmount = discountAmount;


            // Calculate order total without discounts


            return View(shoppingCartVM); // Return the Index view with the updated ShoppingCartVM
                                         //  return RedirectToAction(nameof(OrderConfirmation), new { id = selectedCoupon.Id });
        }

        public IActionResult Discount12(int id)
        {
            // Get user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get shopping cart items for the user
            var shoppingCartItems = unitofwork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            // Get the selected coupon by its ID
            var selectedCoupon = db.coupons.FirstOrDefault(c => c.Id == id);

            // Calculate the discount amount based on the selected coupon
            double discountAmount = 0;
            if (selectedCoupon != null)
            {
                foreach (var cartItem in shoppingCartItems)
                {
                    cartItem.Price = GetPrice(cartItem); // Ensure to get the correct price for each cart item

                    if (selectedCoupon.Percentage > 0)
                    {
                        discountAmount += (cartItem.Price * selectedCoupon.Percentage) / 100;
                    }
                    else if (selectedCoupon.DiscountRate > 0)
                    {
                        discountAmount += selectedCoupon.DiscountRate;
                    }
                }
            }

            // Create ShoppingCartVM instance
            var shoppingCartVM = new CouponVM
            {
                ShoppingCartList = shoppingCartItems,
                SelectedCouponId = id, // Set the selected coupon ID
                DiscountedAmount = discountAmount, // Pass the calculated discount amount to the view
                OrderHeader = new OrderHeader(),

                AddressList = db.addresses.ToList().Where(u => u.User_id == userId),
            };

            // Calculate order total with discounts
            foreach (var cartItem in shoppingCartVM.ShoppingCartList)
            {
                shoppingCartVM.OrderHeader.orderTotal += (cartItem.Price * cartItem.Count);
              shoppingCartVM.OrderHeader.orderTotal= shoppingCartVM.OrderHeader.orderTotal-shoppingCartVM.DiscountedAmount;
                shoppingCartVM.OrderHeader.orderDiscount = shoppingCartVM.DiscountedAmount;
            }

           return View(shoppingCartVM); // Return the Index view with the updated ShoppingCartVM
          //  return RedirectToAction(nameof(OrderConfirmation), new { id = selectedCoupon.Id });
        }

        public IActionResult Discounttryorder(int id)
        {
            // Get user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get shopping cart items for the user
            var shoppingCartItems = unitofwork.shoppingCartRepository.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            // Get the selected coupon by its ID
            var selectedCoupon = db.coupons.FirstOrDefault(c => c.Id == id);

            // Calculate the discount amount based on the selected coupon
            double discountAmount = 0;
            if (selectedCoupon != null)
            {
                foreach (var cartItem in shoppingCartItems)
                {
                    cartItem.Price = GetPrice(cartItem); // Ensure to get the correct price for each cart item

                    if (selectedCoupon.Percentage > 0)
                    {
                        discountAmount += (cartItem.Price * selectedCoupon.Percentage) / 100;
                    }
                    else if (selectedCoupon.DiscountRate > 0)
                    {
                        discountAmount += selectedCoupon.DiscountRate;
                    }
                }
            }

            // Calculate order total with discounts
            var orderTotal = 0.0;
            foreach (var cartItem in shoppingCartItems)
            {
                orderTotal += (cartItem.Price * cartItem.Count);
            }
            orderTotal -= discountAmount;

            // Create OrderHeader instance
            var orderHeader = new OrderHeader
            {
                orderTotal = orderTotal
            };

            // Redirect to OrderConfirmation action with coupon ID
            return RedirectToAction(nameof(OrderConfirmation), new { id = selectedCoupon?.Id, orderTotal = orderTotal });
        }
		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new()
			{
				ShoppingCartList = unitofwork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
				   includeProperties: "Product"),
				couponsList = db.cops.ToList(),
                
				AddressList = db.addresses.Where(u => u.User_id == userId).ToList(),
				OrderHeader = new()
			};

			IEnumerable<ProductImage> productImages = unitofwork.productImageRepository.GetAll();

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				// Associate product images with each product
				cart.Product.ProductImages = productImages.Where(u => u.ProductId == cart.Product.Id).ToList();

				// Calculate the price based on quantity or other logic
				cart.Price = GetPrice(cart);

				// Calculate the total order price
				ShoppingCartVM.OrderHeader.orderTotal += (cart.Price * cart.Count);
			}

			return View(ShoppingCartVM);
		}

		public IActionResult Indexhjg()
		{

			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new()
			{
				ShoppingCartList = unitofwork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
				includeProperties: "Product"),
				OrderHeader = new()
			};

			IEnumerable<ProductImage> productImages = unitofwork.productImageRepository.GetAll();

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Product.ProductImages = productImages.Where(u => u.ProductId == cart.Product.Id).ToList();
                
                cart.Price = GetPrice(cart);

				ShoppingCartVM.OrderHeader.orderTotal += (cart.Price * cart.Count);

			}

			return View(ShoppingCartVM);
		}


		public IActionResult Indexex()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Retrieve the user's addresses
            var userAddresses = db.addresses.Where(u => u.User_id == userId).ToList();


            ShoppingCartVM = new()
            {
            ShoppingCartList = unitofwork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
               includeProperties: "Product"),
                couponsList = db.cops.ToList(),
                AddressList=db.addresses.ToList().Where(u=>u.User_id==userId),  
                OrderHeader = new()
            };

            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPrice(cart);
                
                ShoppingCartVM.OrderHeader.orderTotal += (cart.Price*cart.Count);
                
            }

            return View(ShoppingCartVM);
        }




        [HttpGet]
        public IActionResult Summary( int couponid,int SelectedAddressId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = unitofwork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            
            ShoppingCartVM.OrderHeader.ApplicationUser = unitofwork.applicationUser.Get(u => u.Id == userId);

            //ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            //ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            //ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            //ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            //ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            //ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.Postalcode;

            AddressCustomer address = db.addresses.FirstOrDefault(u => u.Id == SelectedAddressId);



            // Check if the address is found
            if (address != null)
            {
                // Assign the address details to the OrderHeader properties
                ShoppingCartVM.OrderHeader.Name = address.FullName;
                ShoppingCartVM.OrderHeader.PhoneNumber = address.Phonenumber;
                ShoppingCartVM.OrderHeader.StreetAddress = address.Area;
                ShoppingCartVM.OrderHeader.City = address.BuildingName;
                ShoppingCartVM.OrderHeader.State = address.State;
                ShoppingCartVM.OrderHeader.PostalCode = address.Postalcode;
            }

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
               cart.Price = GetPrice(cart);
                ShoppingCartVM.OrderHeader.orderTotal += (cart.Price * cart.Count);
                var item = ShoppingCartVM.DiscountedAmount;
                ShoppingCartVM.OrderHeader.orderTotal = ShoppingCartVM.OrderHeader.orderTotal - ShoppingCartVM.DiscountedAmount;

            }

            //extra added by me 141 - 145
            // Calculate order total with discounts
            //foreach (var cartItem in ShoppingCartVM.ShoppingCartList)
            //{
            //    ShoppingCartVM.OrderHeader.orderTotal += (cartItem.Price * cartItem.Count);
            //    ShoppingCartVM.OrderHeader.orderTotal = ShoppingCartVM.OrderHeader.orderTotal - ShoppingCartVM.DiscountedAmount;
            //}
            return View(ShoppingCartVM);  
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST(ShoppingCartVM shopping)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = unitofwork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = unitofwork.applicationUser.Get(u => u.Id == userId);


            //foreach (var cart in ShoppingCartVM.ShoppingCartList)
            //{
            //    cart.Price = GetPrice(cart);
            //    ShoppingCartVM.OrderHeader.orderTotal += (cart.Price * cart.Count);
            //   // ShoppingCartVM.OrderHeader.
            //}
            //extra added by me 141 - 145
            // Calculate order total with discounts
            foreach (var cartItem in ShoppingCartVM.ShoppingCartList)
            {
                ShoppingCartVM.OrderHeader.orderTotal += (cartItem.Price * cartItem.Count);
                ShoppingCartVM.OrderHeader.orderTotal = ShoppingCartVM.OrderHeader.orderTotal - ShoppingCartVM.DiscountedAmount;
            }
            //it is a regular customer 
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            
               unitofwork.orderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
               unitofwork.Save();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetails orderDetail = new()
                {
                    ProductId = cart.Product_Id,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                unitofwork.orderDetailRepository.Add(orderDetail);
                unitofwork.Save();
            }

            //it is a regular customer account and we need to capture payment
            //stripe logic
            var domain = "https://localhost:7259/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + "customer/cart/index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var item in ShoppingCartVM.ShoppingCartList)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            unitofwork.orderHeaderRepository.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            unitofwork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

          //  return RedirectToAction(nameof(OrderConfirmation),new {id=ShoppingCartVM.OrderHeader.Id});

            }




       
		
        //summary discount

		public IActionResult ApplyDiscount(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var shoppingCartItems = unitofwork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
				includeProperties: "Product");

			var selectedCoupon = db.coupons.FirstOrDefault(c => c.Id == id);

			double discountAmount = 0;
			if (selectedCoupon != null)
			{
				foreach (var cartItem in shoppingCartItems)
				{
					cartItem.Price = GetPrice(cartItem);

					if (selectedCoupon.Percentage > 0)
					{
						discountAmount += (cartItem.Price * selectedCoupon.Percentage) / 100;
					}
					else if (selectedCoupon.DiscountRate > 0)
					{
						discountAmount += selectedCoupon.DiscountRate;
					}
				}
			}

            // Redirect to Summary action method with coupon ID and discounted amount as parameters
            // return RedirectToAction("Summary", new { couponid = id, discountedAmount = discountAmount });
            return RedirectToAction("Summary", new { couponid = id, discountedAmount = discountAmount });

        }
        [HttpGet]
        public IActionResult SummaryDiscount1233(int couponid, double discountedAmount, int? selectedAddressId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve the previously selected address ID
            int? previousSelectedAddressId = selectedAddressId;

            // Retrieve the address details of the previously selected address ID
            var selectedAddress = db.addresses.FirstOrDefault(a => a.Id == previousSelectedAddressId);

            // Check if the address is found and populate the order header with address details
            if (selectedAddress != null)
            {
                var shoppingCartVM = new CouponVM
                {
                    ShoppingCartList = db.shoppingCarts
                        .Where(item => item.ApplicationUserId == userId)
                        .ToList(),
                    OrderHeader = new OrderHeader
                    {
                        Name = selectedAddress.FullName,
                        PhoneNumber = selectedAddress.Phonenumber,
                        StreetAddress = selectedAddress.Area,
                        City = selectedAddress.BuildingName,
                        State = selectedAddress.State,
                        PostalCode = selectedAddress.Postalcode
                    },
                    AddressList = db.addresses
                        .Where(address => address.User_id == userId)
                        .ToList(),
                   // pre = previousSelectedAddressId // Pass the previous selected address ID to the view
                };

                return View(shoppingCartVM);
            }
            else
            {
                // Handle the case when the selected address ID is not found
                // Redirect to an error page or return an appropriate response
                return NotFound();
            }
        }

        [HttpGet]
        public IActionResult SummaryDiscount(int couponid, double discountedAmount, int SelectedAddressId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            AddressCustomer address = db.addresses.FirstOrDefault(u => u.Id == SelectedAddressId);

            
            ShoppingCartVM = new()
            {
                ShoppingCartList = unitofwork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new(),
                AddressList = db.addresses.Where(address => address.User_id == userId).ToList()
            };



			ShoppingCartVM.OrderHeader.ApplicationUser = unitofwork.applicationUser.Get(u => u.Id == userId);

			var ShoppingCartVMk = new CouponVM
			{
				ShoppingCartList = db.shoppingCarts.Where(item => item.ApplicationUserId == userId).ToList(),
				OrderHeader = new OrderHeader(), // Ensure that OrderHeader is initialized
				AddressList = db.addresses.Where(address => address.User_id == userId).ToList(),
				//PreviousSelectedAddressId = previousSelectedAddressId // Pass the previous selected address ID to the view
			};

			// Populate the order header with address details only if the address is found
			if (address != null)
			{
				ShoppingCartVM.OrderHeader.Name = address.FullName;


				// Check if the address is found
				
                ShoppingCartVM.OrderHeader.PhoneNumber = address.Phonenumber;
                ShoppingCartVM.OrderHeader.StreetAddress = address.Area;
                ShoppingCartVM.OrderHeader.City = address.BuildingName;
                ShoppingCartVM.OrderHeader.State = address.State;
                ShoppingCartVM.OrderHeader.PostalCode = address.Postalcode;
            }


        



           
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPrice(cart);
                ShoppingCartVM.OrderHeader.orderTotal += (cart.Price * cart.Count);
                ShoppingCartVM.OrderHeader.orderTotal = ShoppingCartVM.OrderHeader.orderTotal - ShoppingCartVM.OrderHeader.orderDiscount;
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("SummaryDiscount")]
        public IActionResult SummaryPOSTDiscount(ShoppingCartVM shopping, int SelectedAddressId)
        {
            // Use the selectedAddressId received from the form submission

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ShoppingCartVM.ShoppingCartList = unitofwork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = unitofwork.applicationUser.Get(u => u.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPrice(cart);
                ShoppingCartVM.OrderHeader.orderTotal += (cart.Price * cart.Count);
            }

            ShoppingCartVM.OrderHeader.orderTotal = ShoppingCartVM.OrderHeader.orderTotal - ShoppingCartVM.DiscountedAmount;

            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

            unitofwork.orderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
            unitofwork.Save();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetails orderDetail = new()
                {
                    ProductId = cart.Product_Id,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                unitofwork.orderDetailRepository.Add(orderDetail);
                unitofwork.Save();
            }

            var domain = "https://localhost:7259/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + "customer/cart/index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var item in ShoppingCartVM.ShoppingCartList)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            unitofwork.orderHeaderRepository.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            unitofwork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }



        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = unitofwork.orderHeaderRepository.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is an order by customer

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitofwork.orderHeaderRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    unitofwork.orderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    unitofwork.Save();
                }
                HttpContext.Session.Clear();

            }

          //  EmailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book",
           //     $"<p>New Order Created - {orderHeader.Id}</p>");

            List<ShoppingCart> shoppingCarts = unitofwork.shoppingCartRepository
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            unitofwork.shoppingCartRepository.RemoveRange(shoppingCarts);
            unitofwork.Save();

            return View(id);
        }
        public IActionResult OrderConfirmationDis(int id)
        {
            OrderHeader orderHeader = unitofwork.orderHeaderRepository.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                //this is an order by customer

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitofwork.orderHeaderRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    unitofwork.orderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    unitofwork.Save();
                }
                HttpContext.Session.Clear();

            }

            //  EmailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book",
            //     $"<p>New Order Created - {orderHeader.Id}</p>");

            List<ShoppingCart> shoppingCarts = unitofwork.shoppingCartRepository
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            unitofwork.shoppingCartRepository.RemoveRange(shoppingCarts);
            unitofwork.Save();

            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            try
            {


                var cartFromDb = unitofwork.shoppingCartRepository
                   .Get(u => u.Id == cartId, includeProperties: "Product");


                if (cartFromDb == null)
                {
                    TempData["Error"] = "Cart item not found.";
                    return RedirectToAction("Index");
                }

                // Ensure that the associated product exists and has Price100 defined
                if (cartFromDb.Product == null || cartFromDb.Product.Price100 == null)
                {
                    TempData["Error"] = "Product details or Price100 not found for the cart item.";
                    return RedirectToAction("Index");
                }

                double
availableQuantity = cartFromDb.Product.Price100 - cartFromDb.Count;


                // Check if Price100 is greater than or equal to Count before incrementing
                if (cartFromDb.Product.Price100 >= cartFromDb.Count && availableQuantity > 0)
                {
                    cartFromDb.Count += 1;

                    // Update the cart item in the repository
                    unitofwork.shoppingCartRepository.Update(cartFromDb);

                    // Save changes to the database
                    unitofwork.Save();

                    TempData["Success"] = "Quantity increased successfully.";
                }
                else
                {
                    TempData["Error"] = $"Cannot increase quantity beyond available limit ({cartFromDb.Product.Price100}).";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the cart.";
                Console.WriteLine(ex);
            }

            return RedirectToAction("Index");
        }

        public IActionResult Pluscorrect(int cartId)
        {
            var cartfromDb=unitofwork.shoppingCartRepository.Get(u=>u.Id==cartId);
            cartfromDb.Count += 1;
            unitofwork.shoppingCartRepository.Update(cartfromDb);
            unitofwork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult minus(int cartId)
        {
            // Include the Product entity
            var cartFromDb = unitofwork.shoppingCartRepository.Get(u => u.Id == cartId, includeProperties: "Product");
            if (cartFromDb == null)
            {
                // Handle case where cart is not found
                return NotFound();
            }

            if (cartFromDb.Product.Price100 >= cartFromDb.Count && cartFromDb.Count >= 0)
            {


                cartFromDb.Count -= 1;
                unitofwork.shoppingCartRepository.Update(cartFromDb);

            }
            if (cartFromDb.Count == 0)
            {

                unitofwork.shoppingCartRepository.Remove(cartFromDb);
            }
            unitofwork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult minuscorrect(int cartId)
        {
            var cartfromDb = unitofwork.shoppingCartRepository.Get(u => u.Id == cartId);
            if (cartfromDb.Count <= 1)
            {
                unitofwork.shoppingCartRepository.Remove(cartfromDb);
            }
            else
            {


                cartfromDb.Count -= 1;
                unitofwork.shoppingCartRepository.Update(cartfromDb);

            }
            unitofwork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult RemoveCorrect(int cartId)
        {
            var cartfromDb = unitofwork.shoppingCartRepository.Get(u => u.Id == cartId);
            
                unitofwork.shoppingCartRepository.Remove(cartfromDb);
           
            unitofwork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = unitofwork.shoppingCartRepository.Get(u => u.Id == cartId);
            unitofwork.shoppingCartRepository.Remove(cartFromDb);



            HttpContext.Session.SetInt32(SD.SessionCart, unitofwork.shoppingCartRepository
              .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPrice(ShoppingCart cart)
        {
            return cart.Product.Price;
        }
        [HttpGet]
        public IActionResult GetAddress(int id)
        {
            // Set the selected address ID in the view model
          //  ShoppingCartVM.SelectedAddressId = id;

            // Redirect to the SummaryDiscount action with the selected address ID as a query parameter
            return RedirectToAction(nameof(SummaryDiscount), new { selectedAddressId = id });
        }

        

            [HttpGet]
        public double GetAddresst(int id)
        {
            ShoppingCartVM.SelectedAddressId = id;
            return id;
        }

    }
}
