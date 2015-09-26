namespace StackCache.Test.Data
{
    using System.Data.Entity;

    public class CustomerContext : DbContext
    {
        public CustomerContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderLine> OrderLines { get; set; }
    }



}