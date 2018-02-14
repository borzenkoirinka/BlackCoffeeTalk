using BlackCoffeeTalk.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackCoffeeTalk.Service.Controllers
{
    public class CoffeeController : Framework.ControllerBase<Coffee>
    {
        private readonly BlackCoffeeTalkDbContext _context;
        public CoffeeController(BlackCoffeeTalkDbContext cmmDbContext) => _context = cmmDbContext;

        protected override IQueryable<Coffee> Read() => _context.Coffees;

        protected override Coffee Create(Coffee model)
        {
            _context.Coffees.Add(model);
            return model;
        }
    }


    public class CityController : Framework.ControllerBase<City>
    {
        private readonly BlackCoffeeTalkDbContext _context;
        public CityController(BlackCoffeeTalkDbContext cmmDbContext) => _context = cmmDbContext;

        protected override IQueryable<City> Read() => _context.Cities;

        protected override City Create(City model)
        {
            _context.Cities.Add(model);
            _context.SaveChanges();
            return model;
        }
    }
}
