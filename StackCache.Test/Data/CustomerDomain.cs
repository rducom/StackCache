namespace StackCache.Test.Data
{
    using ProtoBuf;

    [ProtoContract]
    public class CustomerDomain
    {
        public CustomerDomain()
        {
        }

        public CustomerDomain(Customer customer)
        {
            this.Id = customer.Id;
            this.Name = customer.Name;
        }

        [ProtoMember(1)]
        public int? Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }
    }
}