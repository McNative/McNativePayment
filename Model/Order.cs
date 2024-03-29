﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace McNativePayment.Model
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string OrganisationId { get; set; }

        public Guid? ReferralId { get; set; }

        public string PaymentProvider { get; set; }

        public string PaymentMethod { get; set; }

        public string CheckoutUrl { get; set; }

        public double Amount { get; set; }

        public string ReferenceId { get; set; }

        public string RedirectUrl { get; set; }

        public string CancelUrl { get; set; }

        public DateTime Created { get; set; }

        public DateTime Expiry { get; set; }

        public string Status { get; set; }

        [JsonIgnore]
        [ForeignKey("ReferralId")]
        public virtual Referral Referral { get; set; }


        [JsonIgnore]
        [InverseProperty("Order")]
        public virtual ICollection<OrderProduct> Products { get; set; }
    }
}
