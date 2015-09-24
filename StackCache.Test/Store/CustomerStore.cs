namespace Caching.Test.Store
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using StackCache.Core;
    using StackCache.Core.CacheKeys;
    using StackCache.Core.Stores;


    public class CustomerSource : IDatabaseSourceGlobal<CustomerDomain, int>
    {
        private string connexion = @"Data Source=(LocalDb)\v11.0;Initial Catalog=TESTCACHE;Integrated Security=true";

        public async Task<IEnumerable<CustomerDomain>> Load()
        {
            List<CustomerDomain> customers;
            using (var db = new CustomerContext(this.connexion))
            {
                var loaded = await db.Customers.ToListAsync();
                customers = loaded.Select(i => new CustomerDomain(i)).ToList();
            }
            return customers;
        }

        public async Task Save(IEnumerable<Crud<CustomerDomain>> values)
        {
            using (var db = new CustomerContext(this.connexion))
            {
                foreach (Crud<CustomerDomain> crud in values)
                {
                    switch (crud.Action)
                    {
                        case CrudAction.Insert:
                        case CrudAction.Update:
                            {
                                Customer found = null;
                                if (crud.Value.Id.HasValue)
                                    found = await db.Customers.FirstOrDefaultAsync(i => i.Id.HasValue && i.Id.Value == crud.Value.Id.Value);
                                if (found == null)
                                {
                                    found = new Customer();
                                    db.Customers.Add(found);
                                }
                                found.Name = crud.Value.Name;
                            }
                            break;
                        case CrudAction.Delete:
                            {
                                if (crud.Value.Id.HasValue)
                                {
                                    Customer found = await db.Customers.FirstOrDefaultAsync(i => i.Id.HasValue && i.Id.Value == crud.Value.Id.Value);
                                    if (found != null)
                                        db.Customers.Remove(found);
                                }
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                await db.SaveChangesAsync();
            }
        }

        public Key ToKey(int key)
        {
            return key.ToString();
        }

        public Key ToKey(CustomerDomain value)
        {
            return value.Id.ToString();
        }

        public KeyPrefix Prefix => "Customer";
    }

    public class CustomerStore : Storage<CustomerDomain, int>
    {

        public CustomerStore(ICache cache)
            : base(cache, new CustomerSource())
        {
        }
    }


}