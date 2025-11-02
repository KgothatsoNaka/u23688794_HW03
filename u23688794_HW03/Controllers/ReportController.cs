using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using u23688794_HW03.Models;
using u23688794_HW03.Data;

namespace u23688794_HW03.Controllers
{
    public class ReportController : Controller
    {

        private readonly BikeStoresEntities _context;

        public ReportController()
        {
            _context = new BikeStoresEntities();
            _context.Configuration.ProxyCreationEnabled = false;
            _context.Configuration.LazyLoadingEnabled = false;
        }



        public ActionResult Index()
        {
            var viewModel = new ReportModel
            {
                PopularProducts = new List<ReportModel.PopularProduct>(),
                StaffPerformances = new List<ReportModel.StaffPerformance>(),
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> GenerateReport(DateTime startDate, DateTime endDate, string brandFilter = null, string categoryFilter = null)
        {
            var popularProducts = await GetPopularProductsAsync(startDate, endDate, brandFilter, categoryFilter);
            var staffPerformances = await GetStaffPerformanceAsync(startDate, endDate);

            var model = new ReportModel
            {
                PopularProducts = popularProducts,
                StaffPerformances = staffPerformances,
                StartDate = startDate,
                EndDate = endDate
            };

            return View("Index", model);
        }

        private async Task<List<ReportModel.PopularProduct>> GetPopularProductsAsync(DateTime startDate, DateTime endDate, string brand, string category)
        {
            var query = _context.order_items
                .Include(oi => oi.products)
                .Include(oi => oi.products.brands)
                .Include(oi => oi.products.categories)
                .Include(oi => oi.orders)
                .Where(oi => oi.orders.order_date >= startDate && oi.orders.order_date <= endDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(oi => oi.products.brands.brand_name == brand);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(oi => oi.products.categories.category_name == category);
            }

            var result = await query
                .GroupBy(oi => new
                {
                    oi.products.product_name,
                    oi.products.brands.brand_name,
                    oi.products.categories.category_name
                })
                .Select(g => new ReportModel.PopularProduct
                {
                    product_name = g.Key.product_name,
                    brand_name = g.Key.brand_name,
                    category_name = g.Key.category_name,
                    sales_count = g.Count(),
                    total_quantity_sold = g.Sum(oi => oi.quantity)
                })
                .OrderByDescending(p => p.total_quantity_sold)
                .Take(10)
                .ToListAsync();

            return result;
        }

        [HttpGet]
        private async Task<List<ReportModel.StaffPerformance>> GetStaffPerformanceAsync(DateTime startDate, DateTime endDate)
        {
            var result = await _context.orders
                .Include(o => o.staffs)
                .Include(o => o.staffs.stores)
                .Include(o => o.order_items)
                .Where(o => o.order_date >= startDate && o.order_date <= endDate)
                .GroupBy(o => new
                {
                    o.staffs.staff_id,
                    o.staffs.first_name,
                    o.staffs.last_name,
                    o.staffs.stores.store_name
                })
                .Select(g => new ReportModel.StaffPerformance
                {
                    fullname = g.Key.first_name + " " + g.Key.last_name,
                    store_location = g.Key.store_name,
                    total_orders = g.Count(),
                    total_sales = g.SelectMany(o => o.order_items).Sum(oi => oi.quantity),
                    total_revenue = g.SelectMany(o => o.order_items).Sum(oi => oi.quantity * oi.list_price * (1 - oi.discount))
                })
                .OrderByDescending(s => s.total_revenue)
                .Take(15)
                .ToListAsync();

            return result;
        }

        [HttpGet]
        public async Task<JsonResult> GetArchivedReports()
        {
            var reportsPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/");
            if (!System.IO.Directory.Exists(reportsPath))
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            var files = await Task.Run(() =>
                System.IO.Directory.GetFiles(reportsPath)
                    .Select(f => new
                    {
                        FileName = System.IO.Path.GetFileName(f),
                        FileSize = new System.IO.FileInfo(f).Length,
                        CreatedDate = System.IO.File.GetCreationTime(f)
                    })
                    .OrderByDescending(f => f.CreatedDate)
                    .ToList()
            );

            return Json(files, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> DeleteReport(string fileName)
        {
           
                var filePath = System.Web.Hosting.HostingEnvironment.MapPath($"~/Reports/{fileName}");
                if (System.IO.File.Exists(filePath))
                {
                    await Task.Run(() => System.IO.File.Delete(filePath));
                    return Json(new { success = true });
                }
                return Json(new { success = false, error = "File not found" });
         
        }

        [HttpPost]
        public async Task<JsonResult> SaveReport()
        {
            try
            {
                if (Request.Files.Count == 0)
                    return Json(new { success = false, error = "No file uploaded" });

                var file = Request.Files[0];
                if (file == null || file.ContentLength == 0)
                    return Json(new { success = false, error = "Invalid file" });

                var reportsPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/");
                if (!System.IO.Directory.Exists(reportsPath))
                    System.IO.Directory.CreateDirectory(reportsPath);

                var filePath = System.IO.Path.Combine(reportsPath, file.FileName);

                await Task.Run(() => file.SaveAs(filePath));

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }


    }
}

