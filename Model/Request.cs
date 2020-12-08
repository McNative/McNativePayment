namespace McNativePayment.Model
{
    public class Request
    {
        public string Id { get; set; }

        public string OrganisationId { get; set; }

        public string Provider { get; set; }

        public double Amount { get; set; }

    }
}
