using WebApplication2.Dtos.EventDtos;

namespace WebApplication2.Dtos.OrganizerDtos
{
    public class OrganizerReturnDto
    {
        
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? LogoUrl { get; set; }
        public List<EventInOrganizerDto> Events { get; set; } = new();
    }

    public class EventInOrganizerDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string? BannerImageUrl { get; set; }
    }
}
