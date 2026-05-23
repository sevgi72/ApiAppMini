using WebApplication2.Models.Common;

namespace WebApplication2.Models
{
    public class Ticket:BaseEntity
    {
        public int EventId { get; set; }
        public Event Event { get; set; }

        public string Type { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
