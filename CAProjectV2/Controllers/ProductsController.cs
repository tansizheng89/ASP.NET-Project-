using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAProjectV2.Data;
using CAProjectV2.Models;
using CAProjectV2.Logic;
using Microsoft.AspNetCore.Http;

namespace CAProjectV2.Controllers
{
    public class ProductsController : Controller
    {


        private readonly WebsiteContext _context;

        public ProductsController(WebsiteContext context)
        {
            _context = context;
        }

        //Embedding session into user 
        public async Task<IActionResult> FirstPage()
        {
            string guest = IdGenerator.ID();
            Response.Cookies.Append("GuestLogin", guest);
            User visitor = new User
            {
                Id = guest,
                sessionId = guest.Clone().ToString() 
            };

            await _context.User.AddAsync(visitor);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Products");

        }
        
        // GET: Products

        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["Is_Product"] = "isProductPage";
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("isLogin")))
            {    //checking session is null or not which means checking user log in or out

                var id = HttpContext.Session.GetString("Userid");
                User user = _context.User.AsNoTracking()
                                        .Where(x => x.Id == id).FirstOrDefault();
                ProfileViewModel profile = new ProfileViewModel(user.UserImageUrl, user.FirstName, user.LastName, user.UserName, user.Email, user.PhoneNumber, "", "");
                ViewData["profile"] = profile;

            }

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;
            ViewData["ProductID"] = "ProductName";

            var product = from p in _context.Product
                          select p;

            ViewData["Product"] = product.ToList();
            if (!String.IsNullOrEmpty(Convert.ToString(searchString)))
            {
                product = product.Where(p => Convert.ToString(p.ProductName).Contains(Convert.ToString(searchString))
                                       || Convert.ToString(p.Description).Contains(Convert.ToString(searchString)));
                ViewData["searchProduct"] = product.ToList();
            }
            switch (sortOrder)
            {
                case "name_desc":
                    product = product.OrderByDescending(p => p.ProductName);
                    break;
                default:
                    product = product.OrderBy(p => p.ProductName);
                    break;
            }
            
            string guestLogin = HttpContext.Session.GetString("isLogin");
            ViewData["isLogin"] = guestLogin;

            return View(await product.AsNoTracking().ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Product
                                        .FirstOrDefaultAsync(m => m.Id == id);
            
            if (product == null)
                return NotFound();

            return View(product);
        }

       
    }
}
