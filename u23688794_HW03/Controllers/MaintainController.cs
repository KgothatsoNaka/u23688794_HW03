using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using u23688794_HW03.Data;
using u23688794_HW03.Models;

namespace u23688794_HW03.Controllers
{
    public class MaintainController : Controller
    {
        private readonly BikeStoresEntities _context;

        public MaintainController()
        {
            _context = new BikeStoresEntities();
            _context.Configuration.ProxyCreationEnabled = false;
            _context.Configuration.LazyLoadingEnabled = false;
        }

        public async Task<ActionResult> Maintain(string brandFilter = null, string categoryFilter = null)
        {
            var viewModel = new MaintainViewModel
            {
                StaffList = await GetStaffOrderAsync(),
                CustomerList = await GetCustomersOrderAsync(),
                ProductList = await GetFilteredProductsAsync(brandFilter, categoryFilter),
                Brands = await GetBrandsAsync(),
                Categories = await GetCategoriesAsync(),
                SelectedBrand = brandFilter,
                SelectedCategory = categoryFilter
            };

            return View(viewModel);
        }







        private async Task<List<StaffSummary>> GetStaffOrderAsync()
        {
            var staffList = await _context.staffs
                .Include(s => s.stores)
                .Include(s => s.orders.Select(o => o.order_items.Select(oi => oi.products)))
                .OrderBy(s => s.last_name)
                .ThenBy(s => s.first_name)
                .ToListAsync();

            return staffList.Select(s => new StaffSummary
            {
                staff_id = s.staff_id,
                first_name = s.first_name,
                last_name = s.last_name,
                email = s.email,
                phone = s.phone,
                store_name = s.stores?.store_name ?? "N/A",
                manager_name = GetManagerName(s.manager_id, staffList),
                active = s.active,
                RecentOrders = GetRecentStaffOrders(s.orders)
            }).ToList();
        }

        private string GetManagerName(int? managerId, List<staffs> allStaff)
        {
            if (managerId.HasValue)
            {
                var manager = allStaff.FirstOrDefault(s => s.staff_id == managerId.Value);
                return manager != null ? $"{manager.first_name} {manager.last_name}" : "N/A";
            }
            return "None";
        }

        private List<OrderSummary> GetRecentStaffOrders(ICollection<orders> orders)
        {
            return orders?
                .OrderByDescending(o => o.order_date)
                .Take(3)
                .SelectMany(o => o.order_items.Select(oi => new OrderSummary
                {
                    order_id = o.order_id,
                    order_date = o.order_date.ToString("yyyy-MM-dd"),
                    product_name = oi.products?.product_name ?? "N/A",
                    quantity = oi.quantity,
                    list_price = oi.list_price,
                    discount = oi.discount,
                    total = oi.quantity * oi.list_price * (1 - oi.discount)
                }))
                .ToList() ?? new List<OrderSummary>();
        }



        private async Task<List<CustomerSummary>> GetCustomersOrderAsync()
        {
            var customerList = await _context.customers
                .Include(c => c.orders.Select(o => o.order_items.Select(oi => oi.products)))
                .OrderBy(c => c.last_name)
                .ThenBy(c => c.first_name)
                .Take(60)
                .ToListAsync();

            return customerList.Select(c => new CustomerSummary
            {
                customer_id = c.customer_id,
                first_name = c.first_name,
                last_name = c.last_name,
                email = c.email,
                phone = c.phone,
                street = c.street,
                city = c.city,
                state = c.state,
                RecentOrders = GetRecentCustomerOrders(c.orders)
            }).ToList();
        }

        private List<OrderSummary> GetRecentCustomerOrders(ICollection<orders> orders)
        {
            return orders?
                .OrderByDescending(o => o.order_date)
                .Take(3)
                .SelectMany(o => o.order_items.Select(oi => new OrderSummary
                {
                    order_id = o.order_id,
                    order_date = o.order_date.ToString("yyyy-MM-dd"),
                    product_name = oi.products?.product_name ?? "N/A",
                    quantity = oi.quantity,
                    list_price = oi.list_price,
                    discount = oi.discount,
                    total = oi.quantity * oi.list_price * (1 - oi.discount)
                }))
                .ToList() ?? new List<OrderSummary>();
        }



        private async Task<List<ProductSummaryM>> GetFilteredProductsAsync(string brand, string category)
        {
            var query = _context.products
                .Include(p => p.brands)
                .Include(p => p.categories)
                .Include(p => p.order_items)
                .Select(p => new ProductSummaryM
                {
                    product_id = p.product_id,
                    product_name = p.product_name,
                    BrandName = p.brands.brand_name,
                    CategoryName = p.categories.category_name,
                    model_year = p.model_year,
                    list_price = p.list_price,
                    TotalSold = p.order_items.Any() ? p.order_items.Sum(oi => oi.quantity) : 0
                })
                .AsQueryable();

            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.BrandName == brand);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.CategoryName == category);
            }

            return await query
                .OrderBy(p => p.product_name)
                .ToListAsync();
        }




        private async Task<List<string>> GetBrandsAsync()
        {
            return await _context.brands
                .OrderBy(b => b.brand_name)
                .Select(b => b.brand_name)
                .ToListAsync();
        }

        private async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.categories
                .OrderBy(c => c.category_name)
                .Select(c => c.category_name)
                .ToListAsync();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}