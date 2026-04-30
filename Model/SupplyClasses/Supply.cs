using System;
using System.Collections.Generic;
using System.Linq;

namespace Resonate.Model.SupplyClasses
{
    public class Supply
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int Supplier_id { get; set; }
        public virtual Supplier Supplier { get; set; }
        public DateTime Supply_Date { get; set; }
        private decimal _totalAmount;
        public decimal Total_Amount
        {
            get
            {
                if (Supply_Items != null && Supply_Items.Count > 0)
                    return Supply_Items.Sum(x => x.Quantity * x.Purchase_Price);

                return _totalAmount;
            }
            set
            {
                _totalAmount = value;
            }
        }
        public virtual List<SupplyItem> Supply_Items { get; set; }
    }
}
