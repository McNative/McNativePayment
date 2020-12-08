using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using McNativePayment.Handlers;
using McNativePayment.Model;
using McNativePayment.Model.response;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace McNativePayment.Controllers
{

    //[Authorize]
    [ODataRoutePrefix("Requests")]
    public class RequestsController : ODataController
    {

        private readonly PaymentContext _context;

        public RequestsController(PaymentContext context)
        {
            _context = context;
        }

    }
}
