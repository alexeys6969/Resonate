using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resonate.Model.SaleClasses
{
    public class CreateSaleRequest
    {
        public string Code { get; set; }
        public DateTime? Sale_Date { get; set; }
        public int employee_id { get; set; }
        public List<SaleItemRequest> items { get; set; }
    }

    /// <summary>
    /// Элемент товара в запросе
    /// </summary>
    public class SaleItemRequest
    {
        public int product_id { get; set; }
        public int quantity { get; set; }
        public decimal? price_at_sale { get; set; }
    }

    /// <summary>
    /// Запрос на обновление продажи (полный)
    /// </summary>
    public class UpdateSaleFullRequest
    {
        public SaleUpdateData Sale { get; set; }
        public List<SaleItemUpdateRequest> Items { get; set; }
    }

    /// <summary>
    /// Данные для обновления основной информации о продаже
    /// </summary>
    public class SaleUpdateData
    {
        public string Code { get; set; }
        public int? Employee_id { get; set; }
        public DateTime? Sale_Date { get; set; }
    }

    /// <summary>
    /// Элемент товара для обновления
    /// </summary>
    public class SaleItemUpdateRequest
    {
        public int Id { get; set; }
        public int Product_id { get; set; }
        public int Quantity { get; set; }
        public decimal? Price_At_Sale { get; set; }
        public string Action { get; set; } // "add", "update", "delete"
    }
}
