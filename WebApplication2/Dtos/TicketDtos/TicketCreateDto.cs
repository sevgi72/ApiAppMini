
namespace WebApplication2.Dtos.TicketDtos
{
    public class TicketCreateDto
    {
        public string Type { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        public int EventId { get; set; }
    }
}
