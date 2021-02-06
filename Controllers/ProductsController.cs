using System;
using System.Linq;
using McNativePayment.Model;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;

namespace McNativePayment.Controllers
{

    [ODataRoutePrefix("Products")]
    public class ProductsController : ODataController
    {

        private readonly PaymentContext _context;

        public ProductsController(PaymentContext context)
        {
            _context = context;
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute()]
        public ActionResult<IQueryable<Product>> GetProducts()
        {
            return Ok(_context.Products.Where(p => p.Active));
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("for(ReferenceType={referenceType},ReferenceId={referenceId})")]
        public ActionResult<IQueryable<Product>> GetProductsFor([FromODataUri] string referenceType, [FromODataUri] string referenceId)
        {
            return Ok(_context.ProductAssignments.Where(p => p.ReferenceType == referenceType && p.ReferenceId == referenceId)
                .Select(p => p.Product));
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("({id})")]
        public SingleResult<Product> GetProduct([FromODataUri] Guid id)
        {
            return SingleResult.Create(_context.Products.Where(p => p.Id == id));
        }
    }
}
