using System;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace McNativePayment.Model
{
    public class EdmModel
    {

        public static IEdmModel BuildEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();

            EntityTypeConfiguration<Issuer> issuerType = odataBuilder.EntityType<Issuer>();
            issuerType.Ignore(i => i.Token);

            EntityTypeConfiguration<ProductAssignment> assignmentType = odataBuilder.EntityType<ProductAssignment>();
            assignmentType.Ignore(a => a.Properties);

            odataBuilder.EntitySet<Transaction>("Transactions");

            EntityTypeConfiguration<Product> productType = odataBuilder.EntitySet<Product>("Products").EntityType;
            FunctionConfiguration forFunction = productType.Collection.Function("For");
            forFunction.Parameter<string>("ReferenceType").Required();
            forFunction.Parameter<string>("ReferenceId").Required();
            forFunction.ReturnsFromEntitySet<Product>("Products");

            EntityTypeConfiguration<LicenseActive> oderType =  odataBuilder.EntitySet<LicenseActive>("Orders").EntityType;
            ActionConfiguration coFunction = oderType.Collection.Action("Create");
            coFunction.Parameter<string>("PaymentMethod").Required();
            coFunction.Parameter<string>("OrganisationId").Required();
            coFunction.Parameter<string>("RedirectUrl").Optional();
            coFunction.Parameter<string>("CancelUrl").Optional();
            coFunction.Parameter<string>("Email").Optional();
            coFunction.Parameter<string>("ReferralCode").Optional();
            coFunction.CollectionParameter<Guid>("Products");
            coFunction.ReturnsFromEntitySet<LicenseActive>("Orders");

            return odataBuilder.GetEdmModel();
        }
    }
}
