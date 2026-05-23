using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Extensions;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizerController(ApiAppDbContext apiAppDbContext,IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllOrganizers()
        {
            var organizers = await apiAppDbContext.Organizers
                 .Include(o => o.Events)
                 .ToListAsync();
                var organizerDtos=mapper.Map<List<OrganizerReturnDto>>(organizers);
            return Ok(organizerDtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrganizerCreateDto dto)
        {
            string? fileName = null;

            if (dto.Logo != null)
            {
                if (!dto.Logo.IsImage()) return BadRequest("Only image files are allowed");
                if (!dto.Logo.IsValidSize(5)) return BadRequest("File size must not exceed 5MB");

                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                fileName = await dto.Logo.SaveFileAsync(rootPath);
            }
            var organizer = mapper.Map<Organizer>(dto);

            apiAppDbContext.Organizers.Add(organizer);
            await apiAppDbContext.SaveChangesAsync();
            return Ok(organizer);
        }

        [HttpPost("{id}/logo")]
        public async Task<IActionResult> UploadLogo(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is required");
            if (!file.IsImage()) return BadRequest("Only image files are allowed");
            if (!file.IsValidSize(5)) return BadRequest("File size must not exceed 5MB");

            var organizer = await apiAppDbContext.Organizers.FindAsync(id);
            if (organizer == null) return NotFound("Organizer not found");

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!string.IsNullOrEmpty(organizer.LogoUrl))
            {
                var oldPath = Path.Combine(rootPath, organizer.LogoUrl);
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            organizer.LogoUrl = await file.SaveFileAsync(rootPath);
            await apiAppDbContext.SaveChangesAsync();
            return Ok(new { message = "Logo uploaded", file = organizer.LogoUrl });
        }

        [HttpGet("{organizerId}/events")]
        public async Task<IActionResult> GetEventsByOrganizer(int organizerId)
        {
            var organizerExists = await apiAppDbContext.Organizers.AnyAsync(o => o.Id == organizerId);
            if (!organizerExists) return NotFound("Organizer not found");

            var events = await apiAppDbContext.Events
                .Where(e => e.OrganizerId == organizerId)
                .ToListAsync();
            var eventDtos = mapper.Map<List<EventReturnDto>>(events);
            return Ok(eventDtos);
        }


    }
}
