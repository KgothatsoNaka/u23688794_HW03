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
    public class BikeStoreController : Controller
    {
        private readonly u23688794_HW03.Data.BikeStoresEntities _context;

        public BikeStoreController()
        {
            _context = new u23688794_HW03.Data.BikeStoresEntities();
            _context.Configuration.ProxyCreationEnabled = false;
            _context.Configuration.LazyLoadingEnabled = false;
        }


        public async Task<ActionResult> Index(string brandFilter = null, string categoryFilter = null, int staffPage = 1, int customerPage = 1, int productPage = 1)
        {
            try
            {
                var viewModel = new HomeViewModel
                {
                    StaffList = await GetStaffListAsync(),
                    CustomerList = await GetCustomerListAsync(),
                    ProductList = await GetFilteredProductsAsync(brandFilter, categoryFilter),
                    Brands = await GetBrandsAsync(),
                    Categories = await GetCategoriesAsync(),
                    SelectedBrand = brandFilter,
                    SelectedCategory = categoryFilter
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                var viewModel = new HomeViewModel
                {
                    StaffList = new List<staffs>(),
                    CustomerList = new List<customers>(),
                    ProductList = new List<ProductSummary>(),
                    Brands = new List<string>(),
                    Categories = new List<string>(),
                    SelectedBrand = brandFilter,
                    SelectedCategory = categoryFilter
                };
                return View(viewModel);
            }
        }


        private async Task<List<staffs>> GetStaffListAsync()
        {
            return await _context.staffs
                .Include(s => s.stores)
                .OrderBy(s => s.last_name)
                .ThenBy(s => s.first_name)
                .ToListAsync();
        }

        private async Task<List<customers>> GetCustomerListAsync()
        {
            return await _context.customers
                .OrderBy(c => c.last_name)
                .ThenBy(c => c.first_name)
                .Take(50)
                .ToListAsync();
        }

        private async Task<List<ProductSummary>> GetProductListAsync()
        {
            return await _context.products
                .Include(p => p.brands)
                .Include(p => p.categories)
                .Select(p => new ProductSummary
                {
                    product_name = p.product_name,
                    BrandName = p.brands.brand_name,
                    CategoryName = p.categories.category_name,
                    model_year = p.model_year,
                    list_price = p.list_price
                })
                .OrderBy(p => p.product_name)
                .ToListAsync();
        }

        private async Task<List<ProductSummary>> GetFilteredProductsAsync(string brand, string category)
        {
            var query = _context.products
                .Include(p => p.brands)
                .Include(p => p.categories)
                .Select(p => new ProductSummary
                {
                    product_name = p.product_name,
                    BrandName = p.brands.brand_name,
                    CategoryName = p.categories.category_name,
                    model_year = p.model_year,
                    list_price = p.list_price
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
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}