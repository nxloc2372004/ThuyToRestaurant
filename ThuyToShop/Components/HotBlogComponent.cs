using ThuyTo.Models;
using ThuyTo.SessionSystem;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ThuyTo.Components
{
	[ViewComponent(Name = "HotBlog")]
	public class HotBlogComponent : ViewComponent
	{
		private readonly ThuyToContext _context;
		public HotBlogComponent(ThuyToContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync(string location = "home")
		{
			var listBlog = _context.Blogs.AsNoTracking().OrderByDescending(m => m.BlogViewCount).Where(m => m.IsActive == true && m.IsDeleted == false).Include(m => m.Category).Take(3).ToList();
			ViewBag.Location = location;
			return await Task.FromResult((IViewComponentResult)View("Default", listBlog));
		}
	}
}
