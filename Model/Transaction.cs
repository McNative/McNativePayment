using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using McNativePayment.Utils;

namespace McNativePayment.Model
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required]
        [StringLength(36)]
        public string? FromOrganisationId { get; set; }

        [Required]
        [StringLength(36)]
        public string ToOrganisationId { get; set; }

        [Required]
        [MaxLength(1024)]
        public string Subject { get; set; }

        [Required]
        [Range(0, 10000)]
        public double? AmountOut { get; set; }

        public double AmountIn { get; set; }

        public string Status { get; set; }

        public DateTime Time { get; set; }

        public string? IssuerId { get; set; }

       // public string Signature { get; set; }

        [ForeignKey("IssuerId")]
        public Issuer Issuer { get; set; }

        public string BuildChecksum()
        {
            return Id + FromOrganisationId + ToOrganisationId + Subject + AmountIn + AmountOut + IssuerId + Time.ToUnixTime();
        }
    }
}
