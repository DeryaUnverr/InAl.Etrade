using InAl.Etrade.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InAl.Etrade.ViewComponents
{
    public class CategoryList:ViewComponent
    {
        private readonly ApplicationDbContext _db;
        public CategoryList(ApplicationDbContext db)
        {
            _db = db;
        }

        public IViewComponentResult Invoke()
        {
            var category = _db.Categories.ToList();
            return View(category);
        }
    }
}
