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
using Cop = Bulky.Models.Cop;

namespace BulkyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CouponsControllerextra : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponsControllerextra(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CouponList()
        {
            List<Cop> coupons = _db.cops.ToList();
            return View(coupons);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cop obj)
        {
            // Check if a coupon with the same name already exists
            var existingCoupon = _db.cops.FirstOrDefault(c => c.CouponName == obj.CouponName);
            if (existingCoupon != null)
            {
                ModelState.AddModelError("CouponName", "A coupon with the same name already exists.");
            }

            // Check if the expiry date is valid (not in the past)
            if (obj.ExpiryDate < DateTime.Today)
            {
                ModelState.AddModelError("ExpiryDate", "Expiry date must be a future date.");
            }

            if (ModelState.IsValid)
            {
                _db.cops.Add(obj);
                _db.SaveChanges();
                return RedirectToAction("Index"); // Redirect to the Index action after successful creation
            }

            // If ModelState is not valid, return the view with validation errors
            return View(obj);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Find the coupon to edit by its ID
            var coupon = _db.cops.Find(id);

            if (coupon == null)
            {
                return NotFound(); // Return 404 Not Found if coupon is not found
            }

            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Cop updatedCoupon)
        {
            if (id != updatedCoupon.Id)
            {
                return BadRequest(); // Return bad request if ID does not match model ID
            }

            // Check if a coupon with the updated name already exists (excluding the current coupon)
            var existingCoupon = _db.cops.FirstOrDefault(c => c.Id != id && c.CouponName == updatedCoupon.CouponName);
            if (existingCoupon != null)
            {
                ModelState.AddModelError("CouponName", "A coupon with the same name already exists.");
            }

            // Check if the expiry date is valid (not in the past)
            if (updatedCoupon.ExpiryDate < DateTime.Today)
            {
                ModelState.AddModelError("ExpiryDate", "Expiry date must be a future date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the coupon in the database with the new values
                    var couponToUpdate = _db.cops.Find(id);
                    if (couponToUpdate != null)
                    {
                        couponToUpdate.CouponName = updatedCoupon.CouponName;
                        couponToUpdate.ExpiryDate = updatedCoupon.ExpiryDate;
                        couponToUpdate.Percentage = updatedCoupon.Percentage;

                        _db.SaveChanges();
                    }
                    else
                    {
                        return NotFound(); // Return 404 Not Found if coupon to update is not found
                    }

                    return RedirectToAction("Index"); // Redirect to the Index action after successful update
                }
                catch (Exception)
                {
                    // Handle exceptions if needed
                    return RedirectToAction("Error", "Home"); // Redirect to error page
                }
            }

            // If ModelState is not valid, return the view with validation errors
            return View(updatedCoupon);
        }
    }
}
