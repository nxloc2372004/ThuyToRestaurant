using Microsoft.AspNetCore.Mvc;
using ThuyTo.Models;
using ThuyTo.SessionSystem;

namespace ThuyTo.Areas.Admin.Components
{
    [ViewComponent(Name = "AdminHeader")]
    public class AdminHeaderComponent : ViewComponent
    {
        private readonly ThuyToContext _context;

        public AdminHeaderComponent(ThuyToContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userid = HttpContext.Session.GetInt32(SessionKey.USERID);
            var user = _context.Users.Where(m => m.UserId == userid).FirstOrDefault();
            if (user != null)
            {
                ViewBag.FullName = user.FullName;
                ViewBag.Avatar = user.Avatar;
            }
            return await Task.FromResult((IViewComponentResult)View("Default"));
        }
    }
}
