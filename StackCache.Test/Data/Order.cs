namespace StackCache.Test.Data
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Orders")]
    public class Order
    {
        [Key]
        public int? Id { get; set; }
        public decimal Total { get; set; }
        ICollection<OrderLine> OrderLines { get; set; }
    }
}