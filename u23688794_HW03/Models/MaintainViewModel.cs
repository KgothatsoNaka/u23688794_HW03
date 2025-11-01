using System.Collections.Generic;

namespace u23688794_HW03.Models
{
    public class MaintainViewModel
    {
        public List<StaffSummary> StaffList { get; set; }
        public List<CustomerSummary> CustomerList { get; set; }
        public List<ProductSummaryM> ProductList { get; set; }
        public List<string> Brands { get; set; }
        public List<string> Categories { get; set; }
        public string SelectedBrand { get; set; }
        public string SelectedCategory { get; set; }
    }

    public class StaffSummary
    {
        public int staff_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string store_name { get; set; }
        public string manager_name { get; set; }
        public byte active { get; set; }
        public List<OrderSummary> RecentOrders { get; set; }
    }

    public class CustomerSummary
    {
        public int customer_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public List<OrderSummary> RecentOrders { get; set; }
    }

    public class ProductSummaryM
    {
        public int product_id { get; set; }
        public string product_name { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public short? model_year { get; set; }
        public decimal list_price { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalMade { get; set; }

    }

    public class OrderSummary
    {
        public int order_id { get; set; }
        public string order_date { get; set; }
        public string product_name { get; set; }
        public int quantity { get; set; }
        public decimal discount { get; set; }
        public decimal list_price { get; set; }
        public decimal total { get; set; }
    }
}