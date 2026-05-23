using WebApplication2.Models.Common;

namespace WebApplication2.Models
{
    public class Event:BaseEntity
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string? BannerImageUrl { get; set; }

        public int OrganizerId { get; set; }
        public Organizer Organizer { get; set; }

        public List<Ticket> Tickets { get; set; }
    }
}
