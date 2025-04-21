using ThuyTo.Models;
using ThuyTo.SessionSystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ThuyTo.Components
{
	[ViewComponent(Name = "BestSellerProduct")]
	public class BestSellerProductComponent : ViewComponent
	{
		private readonly ThuyToContext _context;
		public BestSellerProductComponent(ThuyToContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var listBestSellerProduct = _context.Products.AsNoTracking().Where(m => m.IsBestSeller == true && m.IsActive == true && m.IsDeleted == false).Include(m => m.Category).Take(10).ToList();
			return await Task.FromResult((IViewComponentResult)View("Default", listBestSellerProduct));
		}
	}
}
