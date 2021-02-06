using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace McNativePayment.Model
{
    public class LicenseActive
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Key { get; set; }

        public string OrganisationId { get; set; }

        public string LicenseId { get; set; }

        public bool Disabled { get; set; }

        public DateTime? Expiry { get; set; }

        public DateTime ActivationDate { get; set; }

        public DateTime? LastRenewal { get; set; }
    }
}
