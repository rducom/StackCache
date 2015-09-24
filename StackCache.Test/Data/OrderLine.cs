namespace Caching.Test.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("OrderLines")]
    public class OrderLine
    {
        [Key]
        public int? Id { get; set; }
        public string Name { get; set; }
    }
}