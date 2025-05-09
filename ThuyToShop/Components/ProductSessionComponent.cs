﻿using ThuyTo.Models;
using ThuyTo.SessionSystem;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ThuyTo.Components
{
	[ViewComponent(Name = "ProductSession")]
	public class ProductSessionComponent : ViewComponent
	{
		private readonly ThuyToContext _context;
		public ProductSessionComponent(ThuyToContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync(string SessionKey)
		{
			var session = HttpContext.Session;
			string jsonstring = session.GetString(SessionKey);
			var listProduct = new List<Product>();
			if(jsonstring != null)
			{
				listProduct = JsonConvert.DeserializeObject<List<Product>>(jsonstring);
			}
			return await Task.FromResult((IViewComponentResult)View("Default", listProduct));
		}
	}
}
