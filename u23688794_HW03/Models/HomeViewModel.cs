using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using u23688794_HW03.Data;

namespace u23688794_HW03.Models
{
    public class HomeViewModel
    {
        public List<staffs> StaffList { get; set; }
        public List<customers> CustomerList { get; set; }
        public List<ProductSummary> ProductList { get; set; }
        public List<string> Brands { get; set; }
        public List<string> Categories { get; set; }
        public string SelectedBrand { get; set; }
        public string SelectedCategory { get; set; }

    }
}