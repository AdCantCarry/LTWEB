using System.ComponentModel.DataAnnotations;
using TechNova.Models.Auth;

namespace TechNova.Models.Core
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int AddressId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public User User { get; set; }
        public Address Address { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public Payment Payment { get; set; }

    }

}
