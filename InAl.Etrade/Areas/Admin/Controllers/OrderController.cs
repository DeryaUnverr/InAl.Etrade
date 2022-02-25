using InAl.Etrade.Data;
using InAl.Etrade.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InAl.Etrade.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public  OrderDetailsViewModel OrderVM { get; set; }
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpPost]
        [Authorize(Roles =Other.Role_Admin)]
        public IActionResult Onaylandı()
        {
            OrderHeader orderHeader = _db.OrderHeader.FirstOrDefault(x=>x.ID==OrderVM.OrderHeader.ID);
            orderHeader.OrderStatus = Other.Durum_Onaylandı;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Authorize(Roles = Other.Role_Admin)]
        public IActionResult KargoyaVer()
        {
            OrderHeader orderHeader = _db.OrderHeader.FirstOrDefault(x => x.ID == OrderVM.OrderHeader.ID);
            orderHeader.OrderStatus = Other.Durum_Kargoda;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderDetailsViewModel
            {
                OrderHeader = _db.OrderHeader.FirstOrDefault(x => x.ID == id),
                OrderDetails = _db.OrderDetails.Where(x => x.OrderID == id).Include(x => x.Product)
            };
            return View(OrderVM);
        }
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderLİst;
            if (User.IsInRole(Other.Role_Admin))
            {
                orderHeaderLİst = _db.OrderHeader.ToList();
            }
            else
            {
                orderHeaderLİst = _db.OrderHeader.Where(x => x.ApplicationUserID == claim.Value).Include(x => x.ApplicationUser);
            }
            return View(orderHeaderLİst);
        }
        public IActionResult Beklenen()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderLİst;
            if (User.IsInRole(Other.Role_Admin))
            {
                orderHeaderLİst = _db.OrderHeader.Where(x=>x.OrderStatus==Other.Durum_Beklemede);
            }
            else
            {
                orderHeaderLİst = _db.OrderHeader.Where(x => x.ApplicationUserID == claim.Value && x.OrderStatus==Other.Durum_Beklemede).Include(x => x.ApplicationUser);
            }
            return View(orderHeaderLİst);
        }
        public IActionResult Onaylanan()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderLİst;
            if (User.IsInRole(Other.Role_Admin))
            {
                orderHeaderLİst = _db.OrderHeader.Where(x => x.OrderStatus == Other.Durum_Onaylandı);
            }
            else
            {
                orderHeaderLİst = _db.OrderHeader.Where(x => x.ApplicationUserID == claim.Value && x.OrderStatus == Other.Durum_Onaylandı).Include(x => x.ApplicationUser);
            }
            return View(orderHeaderLİst);
        }
        public IActionResult Kargolandı()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderLİst;
            if (User.IsInRole(Other.Role_Admin))
            {
                orderHeaderLİst = _db.OrderHeader.Where(x => x.OrderStatus == Other.Durum_Kargoda);
            }
            else
            {
                orderHeaderLİst = _db.OrderHeader.Where(x => x.ApplicationUserID == claim.Value && x.OrderStatus == Other.Durum_Kargoda).Include(x => x.ApplicationUser);
            }
            return View(orderHeaderLİst);
        }
    }
}
