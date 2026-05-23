namespace WebApplication2.Dtos.OrganizerDtos
{
    public class OrganizerCreateDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public IFormFile? Logo { get; set; }
    }
}
