namespace Resonate.Model.SupplyClasses
{
    public class SupplyItem
    {
        public int Id { get; set; }
        public int Supply_id { get; set; }
        public virtual Supply Supply { get; set; }
        public int Product_id { get; set; }
        public virtual Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Purchase_Price { get; set; }
    }
}
