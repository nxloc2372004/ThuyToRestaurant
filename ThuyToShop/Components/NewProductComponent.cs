using ThuyTo.Models;
using ThuyTo.SessionSystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ThuyTo.Components
{
	[ViewComponent(Name = "NewProduct")]
	public class NewProductComponent : ViewComponent
	{
		private readonly ThuyToContext _context;
		public NewProductComponent(ThuyToContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var listNewProduct = _context.Products.AsNoTracking().OrderByDescending(m => m.ProductViewCount).Where(m => m.IsActive == true && m.IsDeleted == false).Include(m => m.Category).Take(8).ToList();
			return await Task.FromResult((IViewComponentResult)View("Default", listNewProduct));
		}
	}
}
