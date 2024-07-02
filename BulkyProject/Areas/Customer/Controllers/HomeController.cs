using Bulky.DataAccess.Data;
using Bulky.DataAccess.Migrations;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Bulky.Models;
using Bulky.Models.ViewModels;
using System.Drawing;
//using WishList  Bulky.DataAccess.Migrations.WishList;

namespace BulkyProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitOfWork;

        public ApplicationDbContext db { get; }

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork,ApplicationDbContext db)
        {
            _logger = logger;
            this.unitOfWork = unitOfWork;
           this.db = db;
        }
        public IActionResult Index()
        {

                if (HttpContext.User.Identity.IsAuthenticated)
                {

                    if (User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }

                }

                IEnumerable<Product> productList = unitOfWork.product.GetAll(includeProperties: "Category,ProductImages");
                return View(productList);
          
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = unitOfWork.product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages"),
                Count = 1,
                Product_Id = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = unitOfWork.shoppingCartRepository.Get(u => u.ApplicationUserId == userId &&
            u.Product_Id == shoppingCart.Product_Id);

            if (cartFromDb != null)
            {
                //shopping cart exists
                cartFromDb.Count += shoppingCart.Count;
                unitOfWork.shoppingCartRepository.Update(cartFromDb);
                unitOfWork.Save();
            }
            else
            {
                //add cart record
                unitOfWork.shoppingCartRepository.Add(shoppingCart);
                unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            TempData["success"] = "Cart updated successfully";




            return RedirectToAction(nameof(Index));
        }

        public IActionResult Color()
        {
            return View();
        }
        [HttpGet]
        [Authorize]
        public IActionResult FavList(int Product_Id)
        {
            // Get the user ID from the claims
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Retrieve the Product including the ProductImages property
            Product product = unitOfWork.product.Get(u => u.Id == Product_Id, includeProperties: "ProductImages");

            if (product == null)
            {
                return NotFound(); // Handle product not found
            }

            // Check if the product is already in the user's favorite list
            Fav cartFromDb = unitOfWork.favListRepository.Get(u => u.User_id == userId && u.Product_id == Product_Id);

            if (cartFromDb == null)
            {
                // Retrieve the product images
                IEnumerable<ProductImage> productImages = unitOfWork.productImageRepository.GetAll(u => u.ProductId == product.Id);

                // Create a new favorite item
                Fav fav = new Fav()
                {
                    Name = product.Title,
                    Product_id = Product_Id,
                    Image = productImages.Select(pi => pi.ImageURLL).FirstOrDefault(), // Assuming Image is a single image URL
                    User_id = userId,
                };

                // Add the new favorite to the database
                unitOfWork.favListRepository.Add(fav);
                unitOfWork.Save();
            }

            return View();
        }







        [HttpGet]
        [Authorize]
        public IActionResult FavList1(int Product_Id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the current user's ID

            try
            {
                // Retrieve the product including related data
                Product product = unitOfWork.product.Get(u => u.Id == Product_Id, includeProperties: "Category,ProductImages");

                if (product == null)
                {
                    TempData["error"] = "Product not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Retrieve the primary product image
                ProductImage productImage = product.ProductImages?.FirstOrDefault();

                // Check if the product is already in the user's favorite list
                Fav favFromDb = unitOfWork.favListRepository.Get(u => u.User_id == userId && u.Product_id == Product_Id);

                if (favFromDb == null)
                {
                    // Create a new Fav object for the user's favorite
                    Fav newFav = new Fav
                    {
                        Product = product,
                        Name = product.Title,
                        Product_id = Product_Id, // Assuming Product_id is an auto-generated identity column
                      //  Image = productImage?.ImageURLL, // Assign the image URL
                        User_id = userId
                    };

                    // Add the new Fav to the repository
                    unitOfWork.favListRepository.Add(newFav);
                    unitOfWork.Save(); // Save changes to the database

                    TempData["success"] = "Product added to favorites successfully.";
                }
                else
                {
                    TempData["info"] = "Product is already in your favorites.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Failed to add product to favorites: {ex.Message}";
            }

            return RedirectToAction(nameof(Index)); // Redirect to an appropriate view
        }


        [HttpPost]
        public IActionResult AddToFavorites(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IEnumerable<Product> productList = unitOfWork.product.GetAll(includeProperties: "Category,ProductImages");

            // Retrieve the product including related images
            var product = db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                TempData["error"] = "Product not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if the product is already in favorites for the user
            bool isProductInFavorites = db.Favs
                .Any(f => f.Product_id == productId && f.User_id == userId);

            if (!isProductInFavorites)
            {
                // Create a new Fav entry
                var newFav = new Fav
                {
                    Product_id = productId,
                    User_id = userId,
                    Name = product.Title,
                    // Assign the first image URL from the product's images to the Fav
                    //Image = product.ProductImages.FirstOrDefault()?.ImageURLL
                };

                // Add the new Fav to the database
                db.Favs.Add(newFav);
                db.SaveChanges();

                TempData["success"] = "Product added to favorites successfully.";
            }
            else
            {
                TempData["info"] = "Product is already in your favorites.";
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult Display(int id)
        {
            ShoppingCart cart = new()
            {
                Product = unitOfWork.product.Get(u => u.Id == id, includeProperties: "Category,ProductImages"),
                Count = 1,
                Product_Id = id
            }; 

            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult DDisplay(ShoppingCart shopping)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shopping.ApplicationUserId = userId;

            ShoppingCart cartFromDb = unitOfWork.shoppingCartRepository.Get(u => u.ApplicationUserId == userId
            && u.Product_Id == shopping.Product_Id);
            //ShoppingCart sp = new ShoppingCart()
            //{
            //    ApplicationUserId = userId, 
            //};
            //unitOfWork.shoppingCartRepository.Add(sp);
            //db.SaveChanges();
            if (cartFromDb != null)
            //    Product_Id=shopping.Product_Id,

            {
                cartFromDb.Count= cartFromDb.Count + shopping.Count;  // Update the existing cart
             //   unitOfWork.shoppingCartRepository.Update(cartFromDb);  // Update the existing cart in the repository
                db.shoppingCarts.Update(cartFromDb); 
            }
            else
            {
                //unitOfWork.shoppingCartRepository.Add(shopping);  // Add a new cart to the repository
                db.shoppingCarts.Add(shopping);
            }
            
                // Your existing code for updating/adding the shopping cart

                db.SaveChanges(); // Save changes to the database

                return RedirectToAction(nameof(Index));
            
            
        }
        [HttpPost]
        [Authorize]
        public IActionResult Display(ShoppingCart cart)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.ApplicationUserId = userId;

                ShoppingCart cartFromDb = unitOfWork.shoppingCartRepository.Get(u => u.ApplicationUserId == userId
                && u.Product_Id == cart.Product_Id);
                if (cartFromDb != null)
                {
                    cartFromDb.Count = cartFromDb.Count + cart.Count;
                    db.shoppingCarts.Update(cartFromDb);
                }
                else
                {
                    ShoppingCart sp = new ShoppingCart()
                    {
                        Count =  cart.Count,
                    ApplicationUserId = userId,
                        Product_Id = cart.Product_Id,
                    };


                    db.shoppingCarts.Add(sp);
                    HttpContext.Session.SetInt32(SD.SessionCart,
unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId).Count());

                }
                TempData["Success"] = "cart updated successfully";
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return RedirectToAction("Index");
        }
            public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        [Authorize]
        public IActionResult Displaysession(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = unitOfWork.shoppingCartRepository.Get(u => u.ApplicationUserId == userId &&
            u.Product_Id == shoppingCart.Product_Id);

            if (cartFromDb != null)
            {
                //shopping cart exists
                cartFromDb.Count += shoppingCart.Count;
                unitOfWork.shoppingCartRepository.Update(cartFromDb);
                unitOfWork.Save();
            }
            else
            {
                //add cart record
                unitOfWork.shoppingCartRepository.Add(shoppingCart);
                unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            TempData["success"] = "Cart updated successfully";




            return RedirectToAction(nameof(Index));
        }
        public IActionResult Colors()
        {


            return View();
            // return View();
        }
        public IActionResult FilterColorss(string Color)
        {
            IEnumerable<Product> productList = unitOfWork.product
    .GetAll(includeProperties: "Category")
    .Where(product => product.DressColor == Color).ToList();


            return View(productList);
            // return View();
        }
        public IActionResult FilterColor()
        {
            IEnumerable<Product> productList = unitOfWork.product
    .GetAll(includeProperties: "Category")
    .Where(product => product.DressColor == "Blue").ToList();


            return View(productList);
            // return View();
        }
        public IActionResult PriceLowToHigh()
        {
            IEnumerable<Product> query = unitOfWork.product
     .GetAll(includeProperties: "Category").OrderBy(p => p.Price).ToList();
            return View(query);
        }
        public IActionResult PriceHighToLow()
        {
            IEnumerable<Product> query = unitOfWork.product
     .GetAll(includeProperties: "Category").OrderByDescending(p => p.Price).ToList();

            return View(query);
        }
        public IActionResult Asending()
        {
            IEnumerable<Product> query = unitOfWork.product
     .GetAll(includeProperties: "Category").OrderBy(p => p.Title).ToList();
            return View(query);
        }
        public IActionResult Desending()
        {
            IEnumerable<Product> query = unitOfWork.product
     .GetAll(includeProperties: "Category").OrderByDescending(p => p.Title).ToList();
            return View(query);
        }


    }
}
