using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace McNativePayment.Model
{
    public class EdmModel
    {

        public static IEdmModel BuildEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();

            odataBuilder.EntitySet<Transaction>("Transactions");

            /*
             * odataBuilder.EntityType<Transaction>().Action("Sign")
                .ReturnsFromEntitySet<Transaction>("Transactions")
                .Parameter<string>("Signature").Required();
             */

            return odataBuilder.GetEdmModel();
        }
    }
}
