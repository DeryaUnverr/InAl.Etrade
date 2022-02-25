using InAl.Etrade.Data;
using InAl.Etrade.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
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
            var payment = PaymentProcess(model);
            _db.ShopingCards.RemoveRange(ShopingCardVM.ListCard);
            _db.SaveChanges();
            HttpContext.Session.SetInt32(Other.ssShoppingCart, 0);
            return RedirectToAction("SiparisTamam");


        }

        private Payment PaymentProcess(ShopingCardVM model)
        {
            Options options = new Options();
            options.ApiKey = "sandbox-CvsZq4DFfOATFNaOxDszCPRqzIQfOpVc";
            options.SecretKey = "sandbox-yp97m4JDMibewDF4EWJk3fKhPYxPrZsb";
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = new Random().Next(1111,9999).ToString();
            request.Price = model.OrderHeader.OrderTotal.ToString();
            request.PaidPrice = model.OrderHeader.OrderTotal.ToString();
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = "B67832";
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

            //PaymentCard paymentCard = new PaymentCard();
            //paymentCard.CardHolderName = "John Doe";
            //paymentCard.CardNumber = "5528790000000008";
            //paymentCard.ExpireMonth = "12";
            //paymentCard.ExpireYear = "2030";
            //paymentCard.Cvc = "123";
            //paymentCard.RegisterCard = 0;
            //request.PaymentCard = paymentCard;

            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardHolderName = model.OrderHeader.CartName;
            paymentCard.CardNumber = model.OrderHeader.CartNumber;
            paymentCard.ExpireMonth = model.OrderHeader.ExprationMonth;
            paymentCard.ExpireYear =model.OrderHeader.ExprationYear;
            paymentCard.Cvc = model.OrderHeader.Cvc;
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer();
            buyer.Id = model.OrderHeader.ID.ToString();
            buyer.Name = model.OrderHeader.Name;
            buyer.Surname =model.OrderHeader.Surname;
            buyer.GsmNumber = model.OrderHeader.PhoneNumber;
            buyer.Email = "email@email.com";
            buyer.IdentityNumber = "74300864791";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress =model.OrderHeader.Address;
            buyer.Ip = "85.34.78.112";
            buyer.City = model.OrderHeader.City;
            buyer.Country = "Turkey";
            buyer.ZipCode = model.OrderHeader.PostalCode;
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = "Jane Doe";
            shippingAddress.City = "Istanbul";
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = "Jane Doe";
            billingAddress.City = "Istanbul";
            billingAddress.Country = "Turkey";
            billingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //kull.sepetinde olan ürünler getirildi.
            foreach (var item in _db.ShopingCards.Where(x => x.ApplicationUserID == claim.Value).Include(x => x.Product)) 
            {
                basketItems.Add(new BasketItem()
                {
                    Id = item.ID.ToString(),
                    Name = item.Product.Title,
                    Category1 = item.Product.CategoryID.ToString(),
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price=(item.Price*item.Count).ToString()
                }) ;
            }
            request.BasketItems = basketItems;

           return Payment.Create(request, options);
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
