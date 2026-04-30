using System;
using System.Collections.Generic;

namespace Resonate.Model.SupplyClasses
{
    public class CreateSupplyRequest
    {
        public int supplier_id { get; set; }
        public List<SupplyItemRequest> items { get; set; }
    }

    public class SupplyItemRequest
    {
        public int product_id { get; set; }
        public int quantity { get; set; }
    }

    public class UpdateSupplyFullRequest
    {
        public UpdateSupplyInfoRequest Supply { get; set; }
        public List<UpdateSupplyItemFullRequest> Items { get; set; }
    }

    public class UpdateSupplyInfoRequest
    {
        public int? Supplier_id { get; set; }
        public DateTime? Supply_Date { get; set; }
    }

    public class UpdateSupplyItemFullRequest
    {
        public int Id { get; set; }
        public int Product_id { get; set; }
        public int Quantity { get; set; }
        public decimal? Purchase_Price { get; set; }
        public string Action { get; set; }
    }
}
