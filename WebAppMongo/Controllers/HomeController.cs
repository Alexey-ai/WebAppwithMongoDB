using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebAppMongo.Models;

namespace WebAppMongo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService db;

        public HomeController(ProductService context)
        {
            db = context;
        }

        public async Task<IActionResult> Index(FilterViewModel filter)
        {
            var books = await db.GetProducts(filter.MinPrice, filter.MaxPrice, filter.Name);
            var model = new IndexViewModel { Books = books, Filter = filter };
            if (model==null) return Forbid();
            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Book p)
        {
            if (ModelState.IsValid)
            {
                await db.Create(p);
                return RedirectToAction("Index");
            }
            return View(p);
        }
        public async Task<IActionResult> Edit(string id)
        {
            Book p = await db.GetProduct(id);
            if (p == null)
                return NotFound();
            return View(p);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Book p)
        {
            if (ModelState.IsValid)
            {
                await db.Update(p);
                return RedirectToAction("Index");
            }
            return View(p);
        }
        public async Task<IActionResult> Delete(string id)
        {
            await db.Remove(id);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> AttachImage(string id)
        {
            Book p = await db.GetProduct(id);
            if (p == null)
                return NotFound();
            return View(p);
        }
        [HttpPost]
        public async Task<ActionResult> AttachImage(string id, IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                await db.StoreImage(id, uploadedFile.OpenReadStream(), uploadedFile.FileName);
            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> GetImage(string id)
        {
            var image = await db.GetImage(id);
            if (image == null)
            {
                return NotFound();
            }
            return File(image, "image/png");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
