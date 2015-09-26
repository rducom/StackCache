namespace StackCache.Test.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int? Id { get; set; }
        public string Name { get; set; }
    }
}