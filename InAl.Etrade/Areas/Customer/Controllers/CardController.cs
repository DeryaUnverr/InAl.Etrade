using InAl.Etrade.Data;
using InAl.Etrade.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace InAl.Etrade.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        public readonly UserManager<IdentityUser> _userManager;
        [BindProperty()]
        public ShopingCardVM ShopingCardVM { get; set; }





        public CardController(UserManager<IdentityUser> userManager, IEmailSender emailSender, ApplicationDbContext db)
        {
            _db = db;
            _emailSender = emailSender;
            _userManager = userManager;
        }
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShopingCardVM = new ShopingCardVM()
            {
                OrderHeader = new OrderHeader(),
                ListCard = _db.ShopingCards.Where(x => x.ApplicationUserID == claim.Value).Include(y => y.Product)
            };
            foreach (var item in ShopingCardVM.ListCard)
            {
                item.Price = item.Product.Price;
                ShopingCardVM.OrderHeader.OrderTotal += (item.Count * item.Product.Price);

            }
            return View(ShopingCardVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Summary(ShopingCardVM model)
        {

            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShopingCardVM.ListCard = _db.ShopingCards.Where(x=>x.ApplicationUserID==claim.Value).Include(k=>k.Product);
            ShopingCardVM.OrderHeader.OrderStatus = Other.Durum_Beklemede;
            ShopingCardVM.OrderHeader.ApplicationUserID = claim.Value;
            ShopingCardVM.OrderHeader.OrderDate=DateTime.Now;
            _db.OrderHeader.Add(ShopingCardVM.OrderHeader);
            _db.SaveChanges();
            foreach (var item in ShopingCardVM.ListCard)
            {
                item.Price = item.Product.Price;
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductID = item.ProductID,
                    OrderID = ShopingCardVM.OrderHeader.ID,
                    Price = item.Price,
                    Count = item.Count
                };
                ShopingCardVM.OrderHeader.OrderTotal += item.Count * item.Product.Price;
                model.OrderHeader.OrderTotal += item.Count * item.Product.Price;
                _db.OrderDetails.Add(orderDetails);
            }
            _db.ShopingCards.RemoveRange(ShopingCardVM.ListCard);
            _db.SaveChanges();
            HttpContext.Session.SetInt32(Other.ssShoppingCart, 0);
            return RedirectToAction("SiparisTamam");


        }
        public IActionResult SiparisTamam()
        {
            return View();
        }
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShopingCardVM = new ShopingCardVM()
            {
                OrderHeader = new OrderHeader(),
                ListCard = _db.ShopingCards.Where(x => x.ApplicationUserID == claim.Value).Include(y => y.Product)
            };
            ShopingCardVM.OrderHeader.OrderTotal = 0;
            ShopingCardVM.OrderHeader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(k => k.Id == claim.Value);
            foreach (var item in ShopingCardVM.ListCard)
            {
                ShopingCardVM.OrderHeader.OrderTotal += (item.Count * item.Product.Price);

            }
            return View(ShopingCardVM);
        }
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _db.ApplicationUsers.FirstOrDefault(x=>x.Id==claim.Value);
            if (user==null)
            {
                ModelState.AddModelError(string.Empty, "Doğrulama Emaili Boş");
            }
            

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code},
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            ModelState.AddModelError(string.Empty, "Email Doğrulama KodunGönder..");
            return RedirectToAction("Success");

        }
        public IActionResult Success()
        {
            return View();
        }
        public IActionResult Add(int cartID)
        {
            var cart = _db.ShopingCards.FirstOrDefault(x => x.ID == cartID);
            cart.Count += 1;
            _db.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Decrease(int cartID)
        {
            var cart = _db.ShopingCards.FirstOrDefault(x => x.ID == cartID);
            if (cart.Count==1)
            {
                var count = _db.ShopingCards.Where(u=>u.ApplicationUserID==cart.ApplicationUserID).ToList().Count();
                _db.ShopingCards.Remove(cart);
                _db.SaveChanges();
                HttpContext.Session.SetInt32(Other.ssShoppingCart, count - 1);
            }
            else
            {
                cart.Count -= 1;
                _db.SaveChanges();
            }
         

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartID)
        {
            var cart = _db.ShopingCards.FirstOrDefault(x => x.ID == cartID);
          
                var count = _db.ShopingCards.Where(u => u.ApplicationUserID == cart.ApplicationUserID).ToList().Count();
                _db.ShopingCards.Remove(cart);
                _db.SaveChanges();
                HttpContext.Session.SetInt32(Other.ssShoppingCart, count - 1);
        


            return RedirectToAction(nameof(Index));
        }
    }
}
