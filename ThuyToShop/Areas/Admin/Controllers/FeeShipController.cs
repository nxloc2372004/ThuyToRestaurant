using ThuyTo.Models;
using ThuyTo.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

namespace ThuyTo.Areas.Admin.Controllers
{
	[Area("Admin")] 

    public class FeeShipController : Controller
	{
		private readonly ThuyToContext _context;
		public FeeShipController(ThuyToContext context)
		{
			_context = context;
		}
		public IActionResult Index()
		{
			var listProvince = _context.Provinces.ToList();
			ViewBag.Provinces = new SelectList(listProvince.ToList(), "ProvinceId", "ProvinceName");
			return View();
		}
		public IActionResult ChooseLocation(int id, string action)
		{
			try
			{
				var data = "";
				if(action == "province")
				{
					var listDistricts = _context.Districts.Where(m => m.ProvinceId == id).ToList();
					if(listDistricts.Count> 0)
					{
						foreach(var item in listDistricts)
						{
							data += "<option value='" + item.DistrictId + "'>" + item.DistrictName + "</option>";
						}
					}
				}
				if(action == "district")
				{
                    var listCommune = _context.Communes.Where(m => m.DistrictId == id).ToList();
                    if (listCommune.Count > 0)
                    {
                        foreach (var item in listCommune)
                        {
                            data += "<option value='" + item.CommuneId + "'>" + item.CommuneName + "</option>";
                        }
                    }
                }
                return Json(new
                {
					status = 0,
					message = "success",
					content = data
                });
            } catch
			{
                return Json(new
                {
					status = 1,
					message = "error from server"
                });
            }
		}
        [AdminAuthentication]
        public IActionResult AddFeeship(int provinceid, int districtsid, int communeid, int? shipprice)
		{
			try
			{
				if(districtsid == 0 || communeid == 0 || shipprice == null)
				{
					return Json(new
					{
						status = 1,
						message = "Invalid form"
					});
				}

				var IsExists = _context.FeeShips.Where(m => m.ProvinceId == provinceid && m.DistrictId == districtsid && m.CommuneId== communeid).FirstOrDefault();
				if(IsExists == null)
				{
                    var feeship = new FeeShip();
                    feeship.ProvinceId = provinceid;
                    feeship.DistrictId = districtsid;
                    feeship.ShipPrice = shipprice;
                    feeship.CommuneId = communeid;
                    _context.FeeShips.Add(feeship);
                    _context.SaveChanges();
					return Json(new
					{
						status = 0,
						message = "Đã thêm mới phí vận chuyển"
					}) ;
                } else
				{
					IsExists.ShipPrice = shipprice;
                    _context.FeeShips.Update(IsExists);
                    _context.SaveChanges();
                    return Json(new
                    {
                        status = 0,
                        message = "Đã cập nhật phí vận chuyển"
                    });
                }
				
            } catch
			{
                return Json(new
                {
                    status = 2,
                    message = "Error from server"
                });
            }
		}
        [HttpPost]
        [AdminAuthentication]
        public IActionResult UpdateFeeship(long id, int? shipprice)
        {
            try
            {
                if (shipprice == null || shipprice < 0)
                {
                    return Json(new { status = 1, message = "Phí vận chuyển không hợp lệ" });
                }

                var item = _context.FeeShips.Find(id);
                if (item == null)
                {
                    return Json(new { status = 1, message = "Không tìm thấy phí vận chuyển" });
                }

                item.ShipPrice = shipprice;
                _context.FeeShips.Update(item);
                _context.SaveChanges();

                return Json(new { status = 0, message = "Đã cập nhật phí vận chuyển" });
            }
            catch (Exception ex)
            {
                return Json(new { status = 1, message = "Lỗi máy chủ: " + ex.Message });
            }
        }


        [AdminAuthentication]
        public IActionResult LoadFeeship()
        {
            try
            {
                var data = "";
                var listFeeship = _context.FeeShips
                    .AsNoTracking()
                    .Include(m => m.Province)
                    .Include(m => m.Commune)
                    .Include(m => m.District)
                    .ToList();

                if (listFeeship.Any())
                {
                    int i = 1;
                    foreach (var item in listFeeship)
                    {
                        data += $@"
                <tr>
                    <td>{i++}</td>
                    <td>{item?.Province?.ProvinceName}</td>
                    <td>{item?.District?.DistrictName}</td>
                    <td>{item?.Commune?.CommuneName}</td>
                    <td>
                        <input type='number' style='width: 100px' value='{item?.ShipPrice}' class='shipprice-input' data-id='{item.FeeShipId}' />
                    </td>
                    <td>
                        <button class='btn btn-sm btn-danger btn-delete-feeship' data-id='{item.FeeShipId}'>
                            <i class='bx bx-trash'></i>
                        </button>
                    </td>
                </tr>";
                    }
                }

                return Json(new { status = 0, message = "success", content = data });
            }
            catch
            {
                return Json(new { status = 1, message = "error from server" });
            }
        }

        [AdminAuthentication]
        public async Task<IActionResult> DeletePernament(long? IdToDelete)
        {
            if (IdToDelete == null)
            {
                return new JsonResult(new
                {
                    message = "Can not find id",
                    status = 1
                });
            }
            try
            {
                var item = await _context.FeeShips.FindAsync(IdToDelete);
                if (item == null)
                {
                    return new JsonResult(new
                    {
                        message = "Can not find item",
                        status = 1
                    });
                }
                else
                {
                    _context.FeeShips.Remove(item);
                    _context.SaveChanges();
                    return new JsonResult(new
                    {
                        message = "Xóa thành công",
                        status = 0
                    });
                }
            }
            catch
            {
                return new JsonResult(new
                {
                    message = "Lỗi",
                    status = 1
                });
            }
        }
    }
}
