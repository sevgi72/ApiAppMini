using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Common;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Extensions;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController(ApiAppDbContext apiAppDbContext, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await apiAppDbContext.Events
                .Include(e => e.Organizer)
                .ToListAsync();

            var eventDtos = mapper.Map<List<EventReturnDto>>(events);
            return Ok(ApiResponse<List<EventReturnDto>>.Ok(eventDtos));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await apiAppDbContext.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound(ApiResponse<EventReturnDto>.Fail("Event not found"));

            var dto = mapper.Map<EventReturnDto>(ev);
            return Ok(ApiResponse<EventReturnDto>.Ok(dto));
        }

        [HttpPost]
        public async Task<IActionResult> Post(EventCreateDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponse<EventReturnDto>.Fail("Data cannot be null"));

            var ev = mapper.Map<Event>(dto);
            apiAppDbContext.Events.Add(ev);
            await apiAppDbContext.SaveChangesAsync();

            var returnDto = mapper.Map<EventReturnDto>(ev);
            return Ok(ApiResponse<EventReturnDto>.Ok(returnDto, "Event created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EventCreateDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponse<EventReturnDto>.Fail("Data cannot be null"));

            var ev = await apiAppDbContext.Events.FindAsync(id);
            if (ev == null)
                return NotFound(ApiResponse<EventReturnDto>.Fail("Event not found"));

            mapper.Map(dto, ev);
            await apiAppDbContext.SaveChangesAsync();

            var returnDto = mapper.Map<EventReturnDto>(ev);
            return Ok(ApiResponse<EventReturnDto>.Ok(returnDto, "Event updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await apiAppDbContext.Events.FindAsync(id);
            if (ev == null)
                return NotFound(ApiResponse<string>.Fail("Event not found"));

            apiAppDbContext.Events.Remove(ev);
            await apiAppDbContext.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("Event deleted successfully"));
        }

        [HttpPost("{id}/banner")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            if (file == null)
                return BadRequest(ApiResponse<EventReturnDto>.Fail("File is required"));
            if (!file.IsImage())
                return BadRequest(ApiResponse<EventReturnDto>.Fail("Only image files are allowed"));
            if (!file.IsValidSize(2))
                return BadRequest(ApiResponse<EventReturnDto>.Fail("File size must be less than 2MB"));

            var ev = await apiAppDbContext.Events.FindAsync(id);
            if (ev == null)
                return NotFound(ApiResponse<EventReturnDto>.Fail("Event not found"));

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            string fileName = await file.SaveFileAsync(rootPath);

            ev.BannerImageUrl = fileName;
            await apiAppDbContext.SaveChangesAsync();

            var dto = mapper.Map<EventReturnDto>(ev);
            return Ok(ApiResponse<EventReturnDto>.Ok(dto, "Banner uploaded successfully"));
        }

        [HttpGet("{eventId}/tickets")]
        public async Task<IActionResult> GetTicketsByEvent(int eventId)
        {
            var eventExists = await apiAppDbContext.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists)
                return NotFound(ApiResponse<List<TicketReturnDto>>.Fail("Event not found"));

            var tickets = await apiAppDbContext.Tickets
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            var ticketDtos = mapper.Map<List<TicketReturnDto>>(tickets);
            return Ok(ApiResponse<List<TicketReturnDto>>.Ok(ticketDtos));
        }

        [HttpGet("{eventId}/organizer")]
        public async Task<IActionResult> GetOrganizerByEvent(int eventId)
        {
            var ev = await apiAppDbContext.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
                return NotFound(ApiResponse<OrganizerReturnDto>.Fail("Event not found"));
            if (ev.Organizer == null)
                return NotFound(ApiResponse<OrganizerReturnDto>.Fail("Organizer not found"));

            var organizerDto = mapper.Map<OrganizerReturnDto>(ev.Organizer);
            return Ok(ApiResponse<OrganizerReturnDto>.Ok(organizerDto));
        }

        [HttpPost("{eventId}/tickets")]
        public async Task<IActionResult> CreateTicketForEvent(int eventId, TicketCreateDto dto)
        {
            var eventExists = await apiAppDbContext.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists)
                return NotFound(ApiResponse<TicketReturnDto>.Fail("Event not found"));

            var ticket = new Ticket
            {
                EventId = eventId,
                Type = dto.Type,
                Price = dto.Price,
                QuantityAvailable = dto.QuantityAvailable
            };

            apiAppDbContext.Tickets.Add(ticket);
            await apiAppDbContext.SaveChangesAsync();

            var ticketDto = mapper.Map<TicketReturnDto>(ticket);
            return Ok(ApiResponse<TicketReturnDto>.Ok(ticketDto, "Ticket created successfully"));
        }
    }
}