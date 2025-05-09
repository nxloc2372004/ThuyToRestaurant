﻿using AspNetCoreHero.ToastNotification.Abstractions;
using ThuyTo.Extension;
using ThuyTo.Models;
using ThuyTo.Models.Authentication;
using ThuyTo.Ultilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace ThuyTo.Areas.Admin.Controllers
{
	[Area("Admin")]
	[AdminAuthentication]
	public class AccountController : Controller
	{
		private readonly ThuyToContext _context;
		private readonly INotyfService _notyf;
		public AccountController(ThuyToContext context, INotyfService notyf)
		{
			_context = context;
			_notyf = notyf;
		}
		public class ListRole
		{
			public int RoleId { get; set; }
			public string? RoleName { get; set; }
		}

		public class ActiveStatus
		{
			public int Id { get; set; }
			public string? Name { get; set; }
		}
		/*[AdminAuthentication]*/
		public IActionResult Index(int? page, int? RoleID = 0, int? ActiveStatus = 0)
		{
			var pageNumber = page == null || page <= 0 ? 1 : page.Value;
			var pageSize = 5;
			List<User> listUser = new List<User>();
			if (RoleID == 0 && ActiveStatus == 0)
			{
				listUser = _context.Users.AsNoTracking().OrderByDescending(x => x.UserId).ToList();
			}
			else
			{
				if (RoleID == 0 && ActiveStatus != 0)
				{
					listUser = _context.Users.AsNoTracking().OrderByDescending(x => x.UserId).Where(m => m.IsBlocked == ActiveStatus).ToList();
				}
				if (RoleID != 0 && ActiveStatus == 0)
				{
					listUser = _context.Users.AsNoTracking().OrderByDescending(x => x.UserId).Where(m => m.RoleId == RoleID).ToList();
				}
				if (RoleID != 0 && ActiveStatus != 0)
				{
					listUser = _context.Users.AsNoTracking().OrderByDescending(x => x.UserId).Where(m => m.RoleId == RoleID && m.IsBlocked == ActiveStatus).ToList();
				}
			}

			PagedList<User> models = new PagedList<User>(listUser.AsQueryable(), pageNumber, pageSize);
			ViewBag.currentPage = pageNumber;


			var listRole = new List<ListRole>();
			listRole.Add(new ListRole { RoleId = 0, RoleName = "Chọn quyền truy cập" });
			listRole.Add(new ListRole { RoleId = 1, RoleName = "Admin" });
			listRole.Add(new ListRole { RoleId = 2, RoleName = "Customer" });
			ViewBag.listRoles = new SelectList(listRole.ToList(), "RoleId", "RoleName", RoleID);


			var actives = new List<ActiveStatus>();
			actives.Add(new ActiveStatus { Id = 0, Name = "Chọn trạng thái hoạt động" });
			actives.Add(new ActiveStatus { Id = 1, Name = "Đang hoạt động" });
			actives.Add(new ActiveStatus { Id = 2, Name = "Đã bị khóa" });
			ViewBag.listActiveStatus = new SelectList(actives.ToList(), "Id", "Name", ActiveStatus);

			ViewBag.RoleID = RoleID;
			return View(models);
		}
        public IActionResult Filter(int? RoleID = 0, int ActiveStatus = 0)
        {
            var url = $"/Admin/Account?RoleID={RoleID}&ActiveStatus={ActiveStatus}";
            if (RoleID == 0)
            {
                url = $"/Admin/Account?ActiveStatus={ActiveStatus}";
            }
            if (ActiveStatus == 0)
            {
                url = $"/Admin/Account?RoleID={RoleID}";
            }
            if (RoleID == 0 && ActiveStatus == 0)
            {
                url = "/Admin/Account/";
            }
            return Json(new
            {
                status = 1,
                LinkURL = url
            });
        }
        public IActionResult Create()
        {
            var listRole = new List<ListRole>();
            listRole.Add(new ListRole { RoleId = 2, RoleName = "Khách hàng" });
            listRole.Add(new ListRole { RoleId = 1, RoleName = "Quản trị viên" });
            ViewBag.listRoles = new SelectList(listRole.ToList(), "RoleId", "RoleName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, Microsoft.AspNetCore.Http.IFormFile? avatar)
        {
            if (user == null)
            {
                return NotFound();
            }
            try
            {
                var checkUserName = _context.Users.Where(m => m.UserName == user.UserName).FirstOrDefault();
                if (user.UserName == null) { TempData["UserNameRequired"] = "Vui lòng nhập tài khoản của bạn"; }
                if (user.FullName == null) { TempData["FullNameRequired"] = "Vui lòng nhập họ và tên của bạn"; }
                if (user.Email == null) { TempData["EmailRequired"] = "Vui lòng nhập Email của bạn"; }
                if (user.Phone == null) { TempData["PhoneRequired"] = "Vui lòng nhập số điện thoại của bạn"; }
                if (checkUserName != null) { TempData["UserNameExists"] = "Tài khoản này đã được sử dụng"; }
                if (user?.UserName == null || user.Phone == null || checkUserName != null || user.FullName == null || user.Email == null )
                {
                    var roles = new List<ListRole>();
                    roles.Add(new ListRole { RoleId = 2, RoleName = "Khách hàng" });
                    roles.Add(new ListRole { RoleId = 1, RoleName = "Quản trị viên" });
                    ViewBag.listRoles = new SelectList(roles.ToList(), "RoleId", "RoleName");
                    _notyf.Warning("Vui lòng kiểm tra lại thông tin");
                    return View(user);
                }
                if (avatar != null)
                {
                    string extension = Path.GetExtension(avatar.FileName);
                    string image = Extension.Extensions.ToUrlFriendly(user.FullName) + extension;
                    user.Avatar = await Functions.UploadFile(avatar, @"Users", image.ToLower());
                    user.Avatar = "Users/" + user.Avatar;
                }
                if (string.IsNullOrEmpty(user.Avatar)) user.Avatar = "avatar-default.jpg";
                user.IsBlocked = 1;
                user.IsDeleted = false;
                user.Password = HashPassword.MD5Password("123123");
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
				_notyf.Success("Thêm mới người dùng thành công");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return NotFound();
            }
        }


		public async Task<IActionResult> Edit(long? id)
		{
			if (id == null || _context.Users == null)
			{
				return NotFound();
			}
			try
			{
				var user = await _context.Users.FindAsync(id);
				if (user == null)
				{
					return NotFound();
				}
                var listRole = new List<ListRole>();
                listRole.Add(new ListRole { RoleId = 2, RoleName = "Khách hàng" });
                listRole.Add(new ListRole { RoleId = 1, RoleName = "Quản trị viên" });
                ViewBag.listRoles = new SelectList(listRole.ToList(), "RoleId", "RoleName", user.RoleId);
                return View(user);
			}
			catch
			{
				return NotFound();
			}
		}

		[HttpPost]
		public async Task<IActionResult> Edit(User user, string OldAvatar, IFormFile? avatar)
		{
			if (user == null)
			{
				return NotFound();
			}
			try
			{
                if (user.UserName == null) { TempData["UserNameRequired"] = "Vui lòng nhập tài khoản của bạn"; }
                if (user.Phone == null) { TempData["PhoneRequired"] = "Vui lòng nhập số điện thoại của bạn"; }
                if (user.FullName == null) { TempData["FullNameRequired"] = "Vui lòng nhập họ và tên của bạn"; }
                if (user.Email == null) { TempData["EmailRequired"] = "Vui lòng nhập Email của bạn"; }
                if (user?.UserName == null || user.FullName == null || user.Phone == null || user.Email == null)
                {
                    var roles = new List<ListRole>();
                    roles.Add(new ListRole { RoleId = 2, RoleName = "Khách hàng" });
                    roles.Add(new ListRole { RoleId = 1, RoleName = "Quản trị viên" });
                    ViewBag.listRoles = new SelectList(roles.ToList(), "RoleId", "RoleName");
                    _notyf.Warning("Vui lòng kiểm tra lại thông tin");
                    return View(user);
                }
                if (avatar != null)
                {
                    string extension = Path.GetExtension(avatar.FileName);
                    string image = Extensions.ToUrlFriendly(user.FullName) + extension;
                    user.Avatar = await Functions.UploadFile(avatar, @"Users", image.ToLower());
                    user.Avatar = "Users/" + user.Avatar;
                }
                else
                {
                    user.Avatar = OldAvatar;
                }
                _context.Update(user);
                await _context.SaveChangesAsync();
                _notyf.Success("Cập nhật người dùng thành công");
                return RedirectToAction(nameof(Index));
            }
			catch
			{
                _notyf.Error("Đang bị lỗi, vui lòng kiểm tra lại");
                return NotFound();

			}
		}
		public async Task<IActionResult> Delete(long? UserID)
		{
			if (UserID == null)
			{
				return new JsonResult(new
				{
					message = "Success",
					status = 0
				});
			}
			try
			{
				var user = await _context.Users.FindAsync(UserID);
				if (user == null)
				{
					return new JsonResult(new
					{
						message = "",
						status = 1
					});
				}
				else
				{
					_context.Remove(user);
					_context.SaveChanges();
					return new JsonResult(new
					{
						message = "Success",
						status = 0
					});
				}
			}
			catch
			{
				return new JsonResult(new
				{
					message = "Error",
					status = 1
				});
			}
		}

		public async Task<IActionResult> Block(long? UserID)
		{
			if (UserID == null)
			{
				return new JsonResult(new
				{
					message = "Error",
					status = 1
				});
			}
			try
			{
				var user = await _context.Users.FindAsync(UserID);
				if (user == null)
				{
					return new JsonResult(new
					{
						message = "Error",
						status = 1
					});
				}
				else
				{
					if (user.IsBlocked == 1) user.IsBlocked = 2;
					else user.IsBlocked = 1;

					_context.Update(user);
					_context.SaveChanges();
					if (user.IsBlocked == 1)
					{
						return new JsonResult(new
						{
							message = "Unblock Acc Success",
							status = 0
						});
					}
					else
					{
						return new JsonResult(new
						{
							message = "Block Acc Success",
							status = 0
						});
					}

				}
			}
			catch
			{
				return new JsonResult(new
				{
					message = "Error",
					status = 1
				});
			}
		}
	}
}
