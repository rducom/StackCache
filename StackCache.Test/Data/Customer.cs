namespace Caching.Test.Data
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using ProtoBuf;

    [Table("Customers")]
    public class Customer
    {
        [Key]
        public int? Id { get; set; }
        public string Name { get; set; }
    }

    [ProtoContract]
    public class CustomerDomain
    {
        public CustomerDomain()
        {
        }

        public CustomerDomain(Customer customer)
        {
        }

        [ProtoMember(1)]
        public int? Id { get; set; }
        
        [ProtoMember(2)]
        public string Name { get; set; }
    }
}