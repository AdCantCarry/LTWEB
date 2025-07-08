    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace TechNova.Models.Core
    {
        public class Payment
        {
            [Key]
            public int PaymentId { get; set; }

            [Required]
            public int OrderId { get; set; }

            [ForeignKey("OrderId")]
            public Order Order { get; set; }

            [Required]
            [StringLength(50)]
            public string Method { get; set; } // "COD", "Banking", "Momo", etc.
            public decimal Amount { get; set; }
            public string Status { get; set; } // "Pending", "Paid", "Failed"

            public bool IsPaid { get; set; }

            public DateTime CreatedAt { get; set; }
        }
    }
