﻿using ThuyTo.Models;
using ThuyTo.Models.Authentication;
using ThuyTo.SessionSystem;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ThuyTo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ThuyToContext _context;
        public HomeController(ILogger<HomeController> logger, ThuyToContext context)
        {
            _logger = logger;
            _context = context;

		}

        /*[CustomerAuthentication]*/
        public IActionResult Index()
        {
            var listProduct = _context.Products.OrderByDescending(m => m.ProductViewCount).Take(30).ToList();
            ViewBag.ListBestSellerProduct = listProduct.Where(m => m.IsBestSeller == true && m.IsActive == true && m.IsDeleted == false);
			ViewBag.ListNewProduct = listProduct.Where(m => m.IsActive == true && m.IsDeleted == false).Take(8).ToList();
			return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [Route("/NotFound")]
        public IActionResult PageNotFound()
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