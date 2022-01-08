using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace McNativePayment.Model
{
    public class Referral
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string OrganisationId { get; set; }

        public string Code { get; set; }

        public double Commission { get; set; }

        public bool IsActive { get; set; }
    }
}
