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
    public class OrderDetailsController : Controller
    {
        private readonly WebsiteContext _context;

        public OrderDetailsController(WebsiteContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Checkout(decimal id)
        {
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("isLogin")))
            {
                string userId = HttpContext.Session.GetString("Userid");
                var products = _context.ShoppingCartItem
                                .Where(x => x.UserId == userId);

                List<OrderDetails> orderDetails = new List<OrderDetails>();
                Order order = new Order();
                DateTime localDate = DateTime.Now;

                order.Id = IdGenerator.ID();
                order.Date = localDate.Date;
                order.UserId = userId;
                order.TotalAmount = id;
                order.OrderDetailsId = IdGenerator.ID();

                foreach (var product in products)
                {
                    orderDetails.Add(new OrderDetails
                    {
                        ActivationCode = IdGenerator.ID(),
                        ProductId = product.ProductId,
                        Quantity = 1,
                        IndivProductPrice = product.Product.Price,
                        Order = order
                    });

                }
                await _context.OrderDetails.AddRangeAsync(orderDetails);
                await _context.SaveChangesAsync();

                foreach (var product in products)
                    _context.Remove(product);

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["loginFirst"] = true;
                return RedirectToAction("Index", "LogIn");
            }
        }


        // GET: OrderDetails
        public async Task<IActionResult> Index()
        {
            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("isLogin")))
            {    //checking session is null or not which means checking user log in or out

                var id = HttpContext.Session.GetString("Userid");
                User user = _context.User.AsNoTracking().Where(x => x.Id == id).FirstOrDefault();

                ProfileViewModel profile = new ProfileViewModel(user.UserImageUrl, user.FirstName, user.LastName, user.UserName, user.Email, user.PhoneNumber, "", "");
                ViewData["profile"] = profile;

            }

            if (!String.IsNullOrEmpty(HttpContext.Session.GetString("isLogin")))
            {
                string userId = HttpContext.Session.GetString("Userid");


                var websiteContext = _context.OrderDetails.Include(o => o.Order)
                                                          .Include(o => o.Product)
                                                          .Where(x => x.Order.UserId == userId)
                                                          .OrderBy(x => x.Product.ProductName)
                                                          .ThenBy(x => x.Order.Date);

                websiteContext = websiteContext.OrderByDescending(x => x.Order.Date);

                IQueryable<DataView> lookup = websiteContext
                                              .Select(x => new DataView { Date = x.Order.Date, ProductId = x.ProductId })
                                              .OrderBy(x => x.Date)
                                              .Distinct()
                                              .OrderByDescending(x => x.Date);

                List<DataView> UniqueList = new List<DataView>();
                foreach (var item in lookup)
                {
                    DataView dataViews = new DataView { Date = item.Date, ProductId = item.ProductId };
                    UniqueList.Add(dataViews);
                }

                ViewData["UniqueList"] = UniqueList;
                string guestLogin = HttpContext.Session.GetString("isLogin");
                ViewData["isLogin"] = guestLogin;

                return View(await websiteContext.ToListAsync());
            }
            else
            {
                return RedirectToAction("Index", "LogIn");
            }

        }


    }
}
