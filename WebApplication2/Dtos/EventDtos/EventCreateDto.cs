namespace WebApplication2.Dtos.EventDtos
{
    public class EventCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }= null!;
        public DateTime Date { get; set; }
        public string Location { get; set; }= null!;
        public int OrganizerId { get; set; }
        public IFormFile Photo { get; set; }= null!;
    }
}
