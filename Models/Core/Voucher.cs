using System;
using System.ComponentModel.DataAnnotations;

namespace TechNova.Models.Core
{
    public class Voucher
    {
        public int VoucherId { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(0, 100)]
        public double? DiscountPercent { get; set; } = null;

        [Range(0, double.MaxValue)]
        public decimal MinimumOrderAmount { get; set; } = 0;

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } = 0;

        public bool IsActive { get; set; } = true;

    }

}
