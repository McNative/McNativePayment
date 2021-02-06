using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace McNativePayment.Model
{
    public class ProductAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string ReferenceType { get; set; }

        public string ReferenceId { get; set; }

        public string Properties { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
