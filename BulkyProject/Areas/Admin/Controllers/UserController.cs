using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> userManager;

        public UserController(IUnitOfWork unitOfWork,ApplicationDbContext applicationDb,IUserRepository userRepository, UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            UserRepository = userRepository;
            this.userManager = userManager;
            applicationDb = applicationDb;
        }

        public ApplicationDbContext applicationDb { get; }
        public IUserRepository UserRepository { get; }

        
        public async Task<IActionResult> Index()
        {
            var users = await UserRepository.GetAll();
            var uservm = new UserViewModel();
            uservm.Users = new List<User>();
            foreach (var user in users)
            {
                uservm.Users.Add(new Bulky.Models.ViewModels.User
                {
                    Id = Guid.Parse(user.Id),
                    UserName = user.UserName,
                    Email = user.Email
                });
            }
            return View(uservm);

           
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {var users=await UserRepository.GetAll();
            var uservm = new UserViewModel();
            uservm.Users = new List<User>();
            foreach(var user in users) {
                uservm.Users.Add(new Bulky.Models.ViewModels.User
                {
                    Id = Guid.Parse(user.Id),
                    UserName = user.UserName,
                    Email = user.Email
                });
            }
            return View(uservm);
        }

        [HttpPost]
        public async Task<IActionResult> List(UserViewModel userViewModel)
        {

            var identityUser = new IdentityUser
            {

                UserName = userViewModel.UserName,
                Email = userViewModel.Email,


            };
            var identityResult = await userManager.CreateAsync(identityUser, userViewModel.Password);
            if (identityResult is not null)
            {
                if (identityResult.Succeeded)
                {

                    var roles = new List<string> { "User" };
                    if (userViewModel.Admincheck)
                    {
                        roles.Add("Admin");
                    }
                    
                    //identityResult = await userManager.AddToRolesAsync(identityUser,roles);
                    //if (identityResult is not null && identityResult.Succeeded)
                    //{
                    //    return RedirectToAction(nameof(Index));
                    //}



                }
            }
            return RedirectToAction(nameof(Index));
        }
        

        [HttpPost]
        public async Task<IActionResult> Block(Guid id)
        {

            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            List<string> existingUserRoles = (await userManager.GetRolesAsync(user)).ToList();

            if (user.LockoutEnd == null)
            {
                user.LockoutEnd = DateTime.Now.AddYears(100);

                // Save changes to the database
                await userManager.UpdateAsync(user);
            //    applicationDb.SaveChanges();

                // Optionally, if you are using a custom DbContext (applicationDb), you may also want to save changes there
                // applicationDb.SaveChanges();
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UnBlock(Guid id)
        {

            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            List<string> existingUserRoles = (await userManager.GetRolesAsync(user)).ToList();

            if (user.LockoutEnd != null)
            {
                user.LockoutEnd = DateTime.Now;

                // Save changes to the database
                await userManager.UpdateAsync(user);
                //    applicationDb.SaveChanges();

                // Optionally, if you are using a custom DbContext (applicationDb), you may also want to save changes there
                // applicationDb.SaveChanges();
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user =
            await userManager.FindByIdAsync(id.ToString());
            if (User is not null)
            {

                var identityResult = await userManager.DeleteAsync(user);

                if (identityResult is not null && identityResult.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                return View();
            }

            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();  
        }
    }
}

