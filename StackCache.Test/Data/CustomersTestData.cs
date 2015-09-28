namespace StackCache.Test.Data
{
    using System.Data.Entity;
    using System.Linq;

    public class CustomersTestData : CreateDatabaseIfNotExists<CustomerContext>
    {
        protected override void Seed(CustomerContext context)
        {
            var customers = Enumerable.Range(0, 10000)
                .Select(i => new Customer { Name = "Customer #" + i.ToString() }).ToList();
            context.Customers.AddRange(customers);
            context.SaveChanges();
            base.Seed(context);
        }
    }
}