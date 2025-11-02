using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u23688794_HW03.Models
{
    public class ReportModel
    {
        public List<PopularProduct> PopularProducts { get; set; }
        public List<StaffPerformance> StaffPerformances { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public class PopularProduct
        {
            public string product_name { get; set; }
            public string brand_name { get; set; }
            public string category_name { get; set; }
            public int sales_count { get; set; }
            public int total_quantity_sold { get; set; }
        }

        public class StaffPerformance
        {
            public string fullname { get; set; }
            public decimal total_revenue { get; set; }

            public string store_location { get; set; }

            public int total_orders { get; set; }
            public int total_sales { get; set; }
        }
    }
}