using InAl.Etrade.Data;
using InAl.Etrade.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InAl.Etrade.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            var products = _db.Products.Where(i=>i.IsHome && i.IsStock).ToList();
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim!=null)
            {
                var count = _db.ShopingCards.Where(x => x.ApplicationUserID == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32(Other.ssShoppingCart, count);
            }
            return View(products);
        }

    
        public IActionResult Details(int id)
        {
            var product = _db.Products.FirstOrDefault(x => x.ID == id);
            ShopingCard card = new ShopingCard()
            {
                Product = product,
                ProductID = product.ID,

            };
            return View(card);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShopingCard sCart)
        {
            sCart.ID = 0;
            if (ModelState.IsValid) //Cart boş ise yeni cart olustur
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                sCart.ApplicationUserID = claim.Value;
                ShopingCard cart = _db.ShopingCards.FirstOrDefault(u =>
                    u.ApplicationUserID == sCart.ApplicationUserID &&
                    u.ProductID == sCart.ProductID);
                if (cart == null)
                {
                    _db.ShopingCards.Add(sCart);
                }
                else
                {
                    cart.Count += sCart.Count;
                }

                _db.SaveChanges();
                var count = _db.ShopingCards.Where(x => x.ApplicationUserID == sCart.ApplicationUserID).ToList().Count();
                HttpContext.Session.SetInt32(Other.ssShoppingCart, count);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var product = _db.Products.FirstOrDefault(i => i.ID == sCart.ID);
                ShopingCard cart = new ShopingCard()
                {
                    Product = product,
                    ProductID = product.ID,

                };

            }

            return View(sCart);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
