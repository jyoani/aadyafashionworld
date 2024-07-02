using Bulky.DataAccess.Data;
using Bulky.DataAccess.Migrations;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Policy;

namespace BulkyProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AddressController : Controller
    {
        private readonly ApplicationDbContext applicationDb;

        public AddressController(ApplicationDbContext applicationDb)
        {
            this.applicationDb = applicationDb;
        }

        public IActionResult Index()
        {
            return View();
        }
        //[HttpPost]
        //public IActionResult AddAddress(Address add)
        //{
        //    var claimsIdentity = (ClaimsIdentity)User.Identity;
        //    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
        //    applicationDb.addresses.Add(add);
        //    applicationDb.SaveChanges();

        //    return View();
        //}
        [HttpGet]
        public IActionResult AddAddress()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult AddAddress(AddressCustomer add)
        {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                // Set the UserId property of the address
                add.User_id = userId;

                try
                {
                    // Add the address to the database
                    applicationDb.addresses.Add(add);
                // applicationDb.SaveChanges();

                try
                {
                    applicationDb.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while saving changes: " + ex.Message);
                    // Handle the exception or rethrow it as needed
                }


                return RedirectToAction("Index", "Home"); // Redirect to home page on successful save
                }
                catch (DbUpdateException ex)
                {
                    // Log the exception
                    Console.WriteLine("Error occurred while saving changes: " + ex.Message);

                    // Optionally, you can log the inner exception as well
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                    }

                    // Redirect to an error action or view
                    return RedirectToAction("ErrorAction");
                }
            
        }
        

        [HttpGet]
        public IActionResult EditAddress(int id)
        {
            // Fetch the address by its ID
            var address = applicationDb.addresses.FirstOrDefault(a => a.Id == id);

            if (address == null)
            {
                // Address not found, handle the scenario accordingly
                return RedirectToAction("NotFoundAction");
            }

            return View(address); // Pass the address details to the view for editing
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult EditAddress(AddressCustomer editedAddress)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing address from the database
                    var existingAddress = applicationDb.addresses.FirstOrDefault(a => a.Id == editedAddress.Id);

                    if (existingAddress == null)
                    {
                        return RedirectToAction("NotFoundAction");
                    }

                    // Update the existing address entity with the edited values
                    existingAddress.FullName = editedAddress.FullName;
                    existingAddress.BuildingName = editedAddress.BuildingName;
                    existingAddress.Phonenumber = editedAddress.Phonenumber;
                    existingAddress.Area = editedAddress.Area; 
                    existingAddress.State = editedAddress.State;    
                    existingAddress.Postalcode  = editedAddress.Postalcode; 
                    // Update other properties as needed

                    applicationDb.addresses.Update(existingAddress);
                    applicationDb.SaveChanges();

                    return RedirectToAction("Index", "Home"); // Redirect to home page on successful update
                }
                catch (DbUpdateException ex)
                {
                    // Log and handle the exception
                    Console.WriteLine("Error occurred while saving changes: " + ex.Message);

                    if (ex.InnerException != null)
                    {
                        Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                    }

                    // Redirect to an error action or view
                    return RedirectToAction("ErrorAction");
                }
            }
            // If ModelState is not valid, return the view with validation errors
            return View(editedAddress);
        }

        [HttpGet]
        public IActionResult List()
        {
            List<AddressCustomer> tags = applicationDb.addresses.ToList();
            return View(tags);
           
        }

        [Authorize]
        public IActionResult ListAddress()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Retrieve the favorites for the current user
            IEnumerable<AddressCustomer> userFavorites = applicationDb.addresses.Where(f => f.User_id == userId).ToList();

            return View(userFavorites);
        }


        [HttpGet]
        public IActionResult ErrorAction()
        {
            return View(); // You may return an error view or perform other actions as needed
        }




    }
}
