using Bulky.DataAccess.Data;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BulkyProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class FavListController : Controller
    {
        private readonly ApplicationDbContext db;

        public FavListController(ApplicationDbContext db)
        {
            this.db = db;
        }
        [Authorize]
        public IActionResult Index123()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            IEnumerable<Fav> productList = db.Favs.ToList();
            return View(productList);
            return View();
        }

        [Authorize]
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Retrieve the favorites for the current user, including product images
            IEnumerable<Fav> userFavorites = db.Favs
                .Include(f => f.Product) // Include the Product navigation property
                .ThenInclude(p => p.ProductImages) // Include the ProductImages navigation property
                .Where(f => f.User_id == userId)
                .ToList();

            return View(userFavorites);
        }


        [Authorize]
        public IActionResult Indexcc()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Retrieve the favorites for the current user
            IEnumerable<Fav> userFavorites = db.Favs.Where(f => f.User_id == userId).ToList();

            return View(userFavorites);

        }
        [Authorize]
        public IActionResult Delete(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Retrieve the favorite to delete
            var favoriteToDelete = db.Favs.FirstOrDefault(f => f.User_id == userId && f.Id == id);

            if (favoriteToDelete == null)
            {
                return NotFound(); // Return 404 if the favorite is not found
            }

            // Remove the favorite from the database
            db.Favs.Remove(favoriteToDelete);
            db.SaveChanges(); // Save changes to the database

            // Redirect back to the Index action to refresh the favorites list
            return RedirectToAction(nameof(Index));
        }


    }
}
