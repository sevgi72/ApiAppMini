using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebApplication2.Controllers;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Models;
using Xunit;

namespace TestProject.ControllerTest
{
    public class EventsControllerTests
    {
        private readonly ApiAppDbContext _context;
        private readonly IMapper _mapper;

        public EventsControllerTests()
        {
            // 1. Hər test üçün sıfırdan təmiz InMemory Database yaradırıq
            var options = new DbContextOptionsBuilder<ApiAppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApiAppDbContext(options);

            // 2. IHttpContextAccessor-un tam mock edilməsi (MapperProfile-ın çökməməsi üçün)
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();

            // Profil daxilində ehtimal olunan Request URL/Host ehtiyaclarını doldururuq
            context.Request.Host = new HostString("localhost:7000");
            context.Request.Scheme = "https";
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            // 3. AutoMapper konfiqurasiyası və Dependency Injection tənzimlənməsi
            var config = new MapperConfiguration(cfg =>
            {
                // Setup MapperProfile which requires IHttpContextAccessor in constructor
                cfg.AddProfile(new WebApplication2.Profiles.MapperProfile(mockHttpContextAccessor.Object));

                // AutoMapper-ə constructor-a bu mock obyekti ötürməsini əmr edirik
                cfg.ConstructServicesUsing(type => mockHttpContextAccessor.Object);
            });

            _mapper = config.CreateMapper();
        }

        #region READ Tests (GetAllEvents & GetById)

        [Fact]
        public async Task GetAllEvents_ReturnsOkResult_WithListOfEvents()
        {
            // Arrange
            var organizer = new Organizer { Id = 1, Name = "Test Organizer", Email = "test@example.com" };
            _context.Organizers.Add(organizer);

            _context.Events.Add(new Event
            {
                Id = 1,
                Title = "Tech Conference",
                Description = "Tech details",
                Location = "Baku",
                Date = DateTime.Now.AddDays(5),
                OrganizerId = 1,
                Organizer = organizer
            });
            await _context.SaveChangesAsync();

            var controller = new EventsController(_context, _mapper);

            // Act
            var result = await controller.GetAllEvents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEvents = Assert.IsAssignableFrom<List<EventReturnDto>>(okResult.Value);
            Assert.Single(returnedEvents);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WithCorrectEvent_WhenEventExists()
        {
            // Arrange
            var organizer = new Organizer { Id = 2, Name = "Tech Org", Email = "tech@test.com" };
            _context.Organizers.Add(organizer);

            _context.Events.Add(new Event
            {
                Id = 42,
                Title = "AI Workshop",
                Location = "Baku",
                OrganizerId = 2,
                Organizer = organizer
            });
            await _context.SaveChangesAsync();

            var controller = new EventsController(_context, _mapper);

            // Act
            var result = await controller.GetById(42);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEvent = Assert.IsType<EventReturnDto>(okResult.Value);
            Assert.Equal("AI Workshop", returnedEvent.Title);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            var controller = new EventsController(_context, _mapper);

            // Act
            var result = await controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region CREATE & UPDATE Tests (Post & Update)

        [Fact]
        public async Task Post_ReturnsOkResult_AndSavesEventToDatabase()
        {
            // Arrange
            var controller = new EventsController(_context, _mapper);
            var dto = new EventCreateDto { Title = "Music Fest", Location = "Baku", OrganizerId = 1 };

            // Act
            var result = await controller.Post(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdEvent = Assert.IsType<Event>(okResult.Value);

            var eventInDb = await _context.Events.FindAsync(createdEvent.Id);
            Assert.NotNull(eventInDb);
            Assert.Equal("Music Fest", eventInDb.Title);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenDtoIsNull()
        {
            // Arrange
            var controller = new EventsController(_context, _mapper);

            // Act
            var result = await controller.Post(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_UpdatesDatabaseField_WhenSuccessful()
        {
            // Arrange
            _context.Events.Add(new Event { Id = 10, Title = "Old Title", Location = "Baku" });
            await _context.SaveChangesAsync();

            var controller = new EventsController(_context, _mapper);
            var updateDto = new EventCreateDto { Title = "Brand New Title", Location = "Ganja", OrganizerId = 1 };

            // Act
            var result = await controller.Update(10, updateDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var eventInDb = await _context.Events.FindAsync(10);
            Assert.Equal("Brand New Title", eventInDb.Title);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            var controller = new EventsController(_context, _mapper);
            var updateDto = new EventCreateDto { Title = "Test", Location = "Baku" };

            // Act
            var result = await controller.Update(999, updateDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region DELETE Tests

        [Fact]
        public async Task Delete_RemovesEvent_WhenEventExists()
        {
            // Arrange
            _context.Events.Add(new Event { Id = 77, Title = "To Be Deleted", Location = "Baku" });
            await _context.SaveChangesAsync();

            var controller = new EventsController(_context, _mapper);

            // Act
            var result = await controller.Delete(77);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var eventInDb = await _context.Events.FindAsync(77);
            Assert.Null(eventInDb);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            var controller = new EventsController(_context, _mapper);

            // Act
            var result = await controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region BANNER UPLOAD Tests

        [Fact]
        public async Task UploadBanner_ReturnsBadRequest_WhenFileIsNull()
        {
            // Arrange
            var controller = new EventsController(_context, _mapper);

            // Act
            var result = await controller.UploadBanner(1, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region TICKETS & ORGANIZER RELATION Tests

        [Fact]
        public async Task GetTicketsByEvent_ReturnsOkResult_WithTickets_WhenEventExists()
        {
            // Arrange
            _context.Events.Add(new Event { Id = 10, Title = "Cinema Night", Location = "Baku" });
            _context.Tickets.Add(new Ticket { Id = 1, EventId = 10, Type = "Standard", Price = 10 });
            await _context.SaveChangesAsync();

            var controller = new EventsController(_context, _mapper);

            // Act
            var result = await controller.GetTicketsByEvent(10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tickets = Assert.IsAssignableFrom<List<TicketReturnDto>>(okResult.Value);
            Assert.Single(tickets);
        }

        [Fact]
        public async Task GetOrganizerByEvent_ReturnsOkResult_WithOrganizer_WhenExists()
        {
            // Arrange
            var organizer = new Organizer { Id = 5, Name = "Main Organizer", Email = "org@test.com" };
            _context.Organizers.Add(organizer);
            _context.Events.Add(new Event { Id = 20, Title = "Concert", Location = "Baku", OrganizerId = 5, Organizer = organizer });
            await _context.SaveChangesAsync();

            var controller = new EventsController(_context, _mapper);

            // Act
            var result = await controller.GetOrganizerByEvent(20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedOrganizer = Assert.IsType<OrganizerReturnDto>(okResult.Value);
            Assert.Equal("Main Organizer", returnedOrganizer.Name);
        }

        [Fact]
        public async Task CreateTicketForEvent_ReturnsOkResult_AndAddsTicketToDb()
        {
            // Arrange
            _context.Events.Add(new Event { Id = 5, Title = "StandUp", Location = "Baku" });
            await _context.SaveChangesAsync();

            var controller = new EventsController(_context, _mapper);
            var ticketDto = new TicketCreateDto { Type = "VIP", Price = 50, QuantityAvailable = 10 };

            // Act
            var result = await controller.CreateTicketForEvent(5, ticketDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdTicket = Assert.IsType<Ticket>(okResult.Value);
            Assert.Equal(5, createdTicket.EventId);
        }

        #endregion
    }
}