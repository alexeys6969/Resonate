using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resonate.Model
{
    public class Product
    {
        public int Id { get; set; }
        public string Article { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Category_Id { get; set; }
        public virtual Category Category { get; set; }
        public decimal Price { get; set; }
        public int Stock_Quantity { get; set; }
    }
}
