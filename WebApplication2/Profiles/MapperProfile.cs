using AutoMapper;
using System;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Extensions;
using WebApplication2.Models;

namespace WebApplication2.Profiles
{
    public class MapperProfile:Profile
    {
        public MapperProfile(IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var uribuilder = new UriBuilder
            {
                Scheme = httpContext.Request.Scheme,
                Host = httpContext.Request.Host.Host,
                Port = httpContext.Request.Host.Port ?? 80
            };
            var url = uribuilder.Uri.AbsoluteUri;

            CreateMap<EventCreateDto, Event>();
               
            CreateMap<TicketCreateDto,Ticket>();
            CreateMap<OrganizerCreateDto, Organizer>();
                

            CreateMap<Organizer, OrganizerReturnDto>()
                 
            CreateMap<Organizer, OrganizerInEventDto>();

            // Ticket mappings
            CreateMap<Ticket, TicketReturnDto>()
                .ForMember(dest => dest.Event, opt => opt.MapFrom(src => src.Event));
            CreateMap<Event, EventInTicketDto>();

            // Event mappings
            CreateMap<Event, EventReturnDto>()
                .ForMember(dest => dest.Organizer, opt => opt.MapFrom(src => src.Organizer));
                
            CreateMap<Event, EventInOrganizerDto>();


        }
    }
}
