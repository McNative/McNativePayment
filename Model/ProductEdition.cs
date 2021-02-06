using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace McNativePayment.Model
{
    public class ProductEdition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }

        public int? Duration { get; set; }

        public bool Active { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
