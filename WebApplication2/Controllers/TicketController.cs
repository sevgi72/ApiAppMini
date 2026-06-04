using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Common;
using WebApplication2.Data;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController(ApiAppDbContext apiAppDbContext, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tickets = await apiAppDbContext.Tickets
                .Include(t => t.Event)
                .ToListAsync();

            var ticketDtos = mapper.Map<List<TicketReturnDto>>(tickets);
            return Ok(ApiResponse<List<TicketReturnDto>>.Ok(ticketDtos));
        }

        [HttpPost]
        public async Task<IActionResult> Create(TicketCreateDto dto)
        {
            if (dto == null)
                return BadRequest(ApiResponse<TicketReturnDto>.Fail("Data cannot be null"));

            var eventExists = await apiAppDbContext.Events.AnyAsync(e => e.Id == dto.EventId);
            if (!eventExists)
                return NotFound(ApiResponse<TicketReturnDto>.Fail("Event not found"));

            var ticket = mapper.Map<Ticket>(dto);
            apiAppDbContext.Tickets.Add(ticket);
            await apiAppDbContext.SaveChangesAsync();

            var returnDto = mapper.Map<TicketReturnDto>(ticket);
            return Ok(ApiResponse<TicketReturnDto>.Ok(returnDto, "Ticket created successfully"));
        }
    }
}