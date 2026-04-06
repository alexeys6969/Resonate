using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resonate.Model.SaleClasses
{
    public class SaleItem
    {
        public int Id { get; set; }
        public int Sale_id { get; set; }
        public virtual Sale Sale { get; set; }
        public int Product_id { get; set; }
        public virtual Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price_At_Sale { get; set; }
    }
}
