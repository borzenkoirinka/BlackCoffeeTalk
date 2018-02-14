using System.Linq;
using BlackCoffeeTalk.Service.Models;
using System.Collections.Generic;
using System.Data.Entity;

namespace BlackCoffeeTalk.Service
{
    public class BlackCoffeeTalkDbContext:DbContext
    {
        private string mainDatabase;

        public BlackCoffeeTalkDbContext(string mainDatabase):base(mainDatabase)
        {
        }

        public DbSet<Coffee> Coffees { get; set; }
        public DbSet<City> Cities { get; set; }
    }
}