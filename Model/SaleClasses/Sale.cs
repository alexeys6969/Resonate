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
        private decimal _totalAmount;
        public decimal Total_Amount
        {
            get
            {
                if (Sale_Items != null && Sale_Items.Count > 0)
                    return Sale_Items.Sum(x => x.Quantity * x.Price_At_Sale);

                return _totalAmount;
            }
            set
            {
                _totalAmount = value;
            }
        }
        public virtual List<SaleItem> Sale_Items { get; set; }
    }
}
