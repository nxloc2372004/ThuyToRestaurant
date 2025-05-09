﻿using ThuyTo.Models;
using ThuyTo.SessionSystem;
using Microsoft.AspNetCore.Mvc;

namespace ThuyTo.Components
{
	[ViewComponent(Name = "HeaderComponent")]
	public class HeaderComponent : ViewComponent
	{
		private readonly ThuyToContext _context;
		public HeaderComponent(ThuyToContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var userid = HttpContext.Session.GetInt32(SessionKey.USERID);
			var user = _context.Users.FirstOrDefault(m => m.UserId == userid);
			if(user != null)
			{
                ViewBag.FullName = user.FullName;
				ViewBag.Avatar = user.Avatar;
				ViewBag.RoleId = user.RoleId;
            }
			var listCategory = _context.Categories.ToList();
			var listCategoryPost = listCategory.Where(m => m.IsActive == true && m.CategoryType == 1);
            var listCategoryProduct = listCategory.Where(m => m.IsActive == true && m.CategoryType == 2);

            ViewBag.ListCategoryPost = listCategoryPost;
            ViewBag.ListCategoryProduct = listCategoryProduct;
            return await Task.FromResult((IViewComponentResult)View("Default"));
		}
	}
}
