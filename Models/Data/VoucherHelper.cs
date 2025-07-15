using TechNova.Models.Core;

namespace TechNova.Models.Data
{
    public static class VoucherHelper
    {
        public static (decimal discount, Voucher? voucher) Validate(StoreDbContext context, string code, decimal total)
        {
            var voucher = context.Vouchers.FirstOrDefault(v => v.Code == code);
            if (voucher == null || !voucher.IsActive || voucher.Quantity <= 0)
                return (0, null);

            if (DateTime.Now < voucher.StartDate || DateTime.Now > voucher.EndDate)
                return (0, null);

            if (total < voucher.MinimumOrderAmount)
                return (0, null);

            decimal discount = 0;

            if (voucher.DiscountPercent.HasValue)
            {
                var raw = total * (decimal)voucher.DiscountPercent.Value / 100;
                discount = voucher.DiscountAmount > 0 ? Math.Min(raw, voucher.DiscountAmount) : raw;
            }
            else
            {
                discount = voucher.DiscountAmount;
            }

            return (discount, voucher);
        }
    }

}
