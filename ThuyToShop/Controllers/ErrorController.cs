using ThuyTo.Models;
using Microsoft.AspNetCore.Mvc;

namespace ThuyTo.Controllers
{
	public class ErrorController : Controller
	{
		private readonly ThuyToContext _context;
		public ErrorController(ThuyToContext context) 
		{ 
			_context = context; 
		}
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult ErrorRole()
		{
			return View();
		}
	}
}
