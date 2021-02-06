using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

    [ODataRoutePrefix("Transactions")]
    [Authorize(AuthenticationSchemes = IssuerAuthenticationHandler.AUTHENTICATION_SCHEMA)]
    public class TransactionsController : ODataController
    {

        private readonly PaymentContext _context;

        public TransactionsController()
        {
            _context = null;
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = IssuerAuthenticationHandler.AUTHENTICATION_SCHEMA)]
        public async Task<ActionResult<object>> CreateTransaction([FromBody] Transaction transaction)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string issuerId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            transaction.Status = "PENDING";
            transaction.AmountIn = transaction.AmountOut.Value * 0.85;
            transaction.Time = DateTime.Now;
            transaction.IssuerId = issuerId;

            if (transaction.AmountOut > 100) transaction.Status = "WAITING_FOR_APPROVAL";
            else transaction.Status = "APPROVED";

            await _context.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return Ok(new TransactionCreateResponse()
            {
                Id = transaction.Id,
                Checksum =  transaction.BuildChecksum()
            });
        }

      //  [HttpPost]
      //  [ODataRoute("{transactionId}/Sign")]
      //  [Authorize(AuthenticationSchemes = IssuerAuthenticationHandler.AUTHENTICATION_SCHEMA)]
        public async Task<ActionResult<Transaction>> SignTransaction([FromODataUri] string transactionId,ODataActionParameters parameters)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Transaction transaction = await _context.Transactions.FindAsync(transactionId);
            if (transaction == null) return NotFound();

            //@Todo check issuer
            string issuerId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (transaction.IssuerId != issuerId || transaction.Status != "PENDING")// || transaction.Signature != null
            {
                return Forbid();
            }

            if (transaction.AmountOut > 100) transaction.Status = "WAITING_FOR_APPROVAL";
            else transaction.Status = "APPROVED";


         //   transaction.Signature = (string) parameters["Signature"];
            string checksum = transaction.BuildChecksum();

            //@Todo validate signature

            _context.Update(transaction);
            await _context.SaveChangesAsync();

            return Ok(transaction);
        }
    }
}
