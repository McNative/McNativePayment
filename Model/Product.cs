using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace McNativePayment.Model
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string OrganisationId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }
        public bool Active { get; set; }

        [InverseProperty("Product")]
        public virtual ICollection<ProductEdition> Editions { get; set; }

        [InverseProperty("Product")]
        public virtual ICollection<ProductAssignment> Assignments { get; set; }
    }
}
