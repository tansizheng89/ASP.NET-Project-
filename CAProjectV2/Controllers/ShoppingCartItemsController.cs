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
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace CAProjectV2.Controllers
{//testing 
    public class ShoppingCartItemsController : Controller
    {
        private readonly WebsiteContext _context;

        public ShoppingCartItemsController(WebsiteContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Adding(ShoppingCartItem items)
        {
            items.Id = IdGenerator.ID();
            items.ShoppingCartId = IdGenerator.ID();
            items.ShoppingCartItemEachProductId = items.Id.Clone().ToString() + items.ShoppingCartId.Clone().ToString();
            items.Quantity = 1;

            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("isLogin")))
                items.UserId = HttpContext.Session.GetString("Userid"); 
            else
                items.UserId = Request.Cookies["GuestLogin"];

            await _context.AddAsync(items);
            await _context.SaveChangesAsync();

            var _exists = _context.WishList
                                    .Where(x => x.UserId == items.UserId
                                    && x.ProductId == items.ProductId)
                                    .FirstOrDefault();

            if (_exists != null)
                _context.WishList.Remove(_exists);

            await _context.SaveChangesAsync();

            return LocalRedirect("/Products/Index");
        }

        public async Task<IActionResult> Addinginwishlist(ShoppingCartItem items)
        {
            items.Id = IdGenerator.ID();
            items.ShoppingCartId = IdGenerator.ID();
            items.ShoppingCartItemEachProductId = items.Id.Clone().ToString() + items.ShoppingCartId.Clone().ToString();
            items.Quantity = 1;
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("isLogin")))
                items.UserId = HttpContext.Session.GetString("Userid");
            else
                items.UserId = Request.Cookies["GuestLogin"];

            await _context.AddAsync(items);
            await _context.SaveChangesAsync();

            var _exists = _context.WishList
                .Where(x => x.UserId == items.UserId
                && x.ProductId == items.ProductId)
                .FirstOrDefault();

            if (_exists != null)
                _context.WishList.Remove(_exists);

            await _context.SaveChangesAsync();

            return LocalRedirect("/WishItem/WishList");
        }

        [HttpPost]
        public async Task<IActionResult> Updating(ShoppingCartItem updatedQty)
        {
         
            var products = _context.ShoppingCartItem
                            .Where(x => x.UserId == updatedQty.UserId 
                                && x.ProductId == updatedQty.ProductId)
                            .ToList();

            //find original qty
            var websiteContext = _context.ShoppingCartItem.Where(x => x.UserId == updatedQty.UserId).Count(x =>
            Convert.ToString(x.ProductId) == Convert.ToString(updatedQty.ProductId));

            if (updatedQty.Quantity > 0)
            {
                //generating new productid details for this updatedQty
                updatedQty.Id = IdGenerator.ID();
                updatedQty.Quantity = 1;
                await _context.ShoppingCartItem.AddAsync(updatedQty);
                await _context.SaveChangesAsync();
                
            }
            else
            {
                _context.ShoppingCartItem.Remove(products.FirstOrDefault());
                await _context.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Index));
        }



        public IActionResult Index()
        {
             if (!String.IsNullOrEmpty(HttpContext.Session.GetString("isLogin")))
            {    //checking session is null or not which means checking user log in or out

                var id = HttpContext.Session.GetString("Userid");
                User user = _context.User.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();
                ProfileViewModel profile = new ProfileViewModel(user.UserImageUrl, user.FirstName, user.LastName, user.UserName, user.Email, user.PhoneNumber, "", "");
                ViewData["profile"] = profile;
             
            }

            string currentLogin;
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("isLogin")))
            {
                currentLogin = HttpContext.Session.GetString("Userid");
            }
            else
            {
                currentLogin = Request.Cookies["GuestLogin"];
            }

  
            var websiteContext = _context.ShoppingCartItem.Include(s => s.Product)
                .Where(x => x.UserId == currentLogin)
                .OrderBy(x => x.Product.ProductName)
                .ToList();

            ViewData["Data"] = websiteContext;
            string guestLogin = HttpContext.Session.GetString("isLogin");
            ViewData["isLogin"] = guestLogin;


            return View();
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string userId, string productId)
        {
            var products = _context.ShoppingCartItem
                    .Where(x => x.UserId == userId
                            && x.ProductId == productId);

            foreach (var product in products)
            {
                _context.ShoppingCartItem.Remove(product);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        //adding action to count the item in shopping cart
        [HttpPost]
        public IActionResult shoppingcartcount()
        {

            string currentLogin;
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("isLogin")))
            {
                currentLogin = HttpContext.Session.GetString("Userid");
            }
            else
            {
                currentLogin = Request.Cookies["GuestLogin"];
            }

      
            var websiteContext = _context.ShoppingCartItem.Include(s => s.Product)
                .Where(x => x.UserId == currentLogin)
                .OrderBy(x => x.Product.ProductName)
                .ToList();
            var count = websiteContext.Count();

            return Json(new { success = count });
        }
    }
}
