using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resonate.Model.SaleClasses
{
    public class Sale
    {
            public int Id { get; set; }
            public string Code { get; set; }
            public int Employee_id { get; set; }
            public virtual Employees Employee { get; set; }
            public DateTime Sale_Date { get; set; }
            public decimal Total_Amount { get; set; }
            public virtual List<SaleItem> Sale_Items { get; set; }
    }
}
