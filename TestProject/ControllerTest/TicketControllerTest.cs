using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApplication2.Controllers;
using WebApplication2.Data;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Models;
using Xunit;

namespace TestProject.ControllerTests
{
    public class TicketControllerTests
    {
        private readonly ApiAppDbContext _context;
        private readonly IMapper _mapper;

        public TicketControllerTests()
        {
            // InMemory Database Setup
            var options = new DbContextOptionsBuilder<ApiAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApiAppDbContext(options);

            // Mock IHttpContextAccessor for MapperProfile
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();

            context.Request.Host = new HostString("localhost:7000");
            context.Request.Scheme = "https";

            mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(context);

            // AutoMapper Configuration
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebApplication2.Profiles.MapperProfile(mockHttpContextAccessor.Object));
                cfg.ConstructServicesUsing(type => mockHttpContextAccessor.Object);
            });

            _mapper = config.CreateMapper();
        }

        #region GET ALL TESTS

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithTickets()
        {
            // Arrange - Database Setup
            var @event = new Event
            {
                Id = 1,
                Title = "Concert",
                Location = "Baku"
            };
            _context.Events.Add(@event);

            var ticket1 = new Ticket
            {
                Id = 1,
                EventId = 1,
                Type = "VIP",
                Price = 50,
                QuantityAvailable = 10
            };
            var ticket2 = new Ticket
            {
                Id = 2,
                EventId = 1,
                Type = "Regular",
                Price = 20,
                QuantityAvailable = 50
            };

            _context.Tickets.Add(ticket1);
            _context.Tickets.Add(ticket2);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            // Act - Execute GetAll
            var result = await controller.GetAll();

            // Assert - Verify Response
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTickets = Assert.IsAssignableFrom<List<TicketReturnDto>>(okResult.Value);

            Assert.Equal(2, returnedTickets.Count);
            Assert.Equal("VIP", returnedTickets[0].Type);
            Assert.Equal("Regular", returnedTickets[1].Type);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithSingleTicket()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);

            var ticket = new Ticket
            {
                Id = 1,
                EventId = 1,
                Type = "Standard",
                Price = 100,
                QuantityAvailable = 5
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tickets = Assert.IsAssignableFrom<List<TicketReturnDto>>(okResult.Value);

            Assert.Single(tickets);
            Assert.Equal(1, tickets[0].Id);
            Assert.Equal("Standard", tickets[0].Type);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithEmptyList()
        {
            // Arrange - No tickets in database
            var controller = new TicketController(_context, _mapper);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tickets = Assert.IsAssignableFrom<List<TicketReturnDto>>(okResult.Value);

            Assert.Empty(tickets);
        }

        [Fact]
        public async Task GetAll_IncludesEventData_WithTickets()
        {
            // Arrange
            var @event = new Event
            {
                Id = 5,
                Title = "Tech Conference",
                Location = "Ganja"
            };
            _context.Events.Add(@event);

            var ticket = new Ticket
            {
                Id = 1,
                EventId = 5,
                Type = "VIP",
                Price = 150,
                QuantityAvailable = 20
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tickets = Assert.IsAssignableFrom<List<TicketReturnDto>>(okResult.Value);

            Assert.Single(tickets);
            Assert.Equal(5, tickets[0].EventId);
        }

        [Fact]
        public async Task GetAll_ReturnsCorrectDtoMapping()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);

            var ticket = new Ticket
            {
                Id = 10,
                EventId = 1,
                Type = "Premium",
                Price = 200,
                QuantityAvailable = 15
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tickets = Assert.IsAssignableFrom<List<TicketReturnDto>>(okResult.Value);

            var returnedTicket = tickets[0];
            Assert.NotNull(returnedTicket);
            Assert.Equal(10, returnedTicket.Id);
            Assert.Equal("Premium", returnedTicket.Type);
            Assert.Equal(200, returnedTicket.Price);
            Assert.Equal(15, returnedTicket.QuantityAvailable);
            Assert.Equal(1, returnedTicket.EventId);
        }

        [Fact]
        public async Task GetAll_ReturnsMultipleTicketsForDifferentEvents()
        {
            // Arrange
            var event1 = new Event { Id = 1, Title = "Event 1", Location = "Baku" };
            var event2 = new Event { Id = 2, Title = "Event 2", Location = "Ganja" };

            _context.Events.Add(event1);
            _context.Events.Add(event2);

            var ticket1 = new Ticket { Id = 1, EventId = 1, Type = "VIP", Price = 100, QuantityAvailable = 10 };
            var ticket2 = new Ticket { Id = 2, EventId = 1, Type = "Regular", Price = 50, QuantityAvailable = 20 };
            var ticket3 = new Ticket { Id = 3, EventId = 2, Type = "Standard", Price = 75, QuantityAvailable = 30 };

            _context.Tickets.Add(ticket1);
            _context.Tickets.Add(ticket2);
            _context.Tickets.Add(ticket3);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tickets = Assert.IsAssignableFrom<List<TicketReturnDto>>(okResult.Value);

            Assert.Equal(3, tickets.Count);
            Assert.Equal(2, tickets.FindAll(t => t.EventId == 1).Count);
            Assert.Equal(1, tickets.FindAll(t => t.EventId == 2).Count);
        }

        #endregion

        #region CREATE TESTS

        [Fact]
        public async Task Create_ReturnsOkResult_WithValidDto()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            var dto = new TicketCreateDto
            {
                EventId = 1,
                Type = "VIP",
                Price = 100,
                QuantityAvailable = 10
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdTicket = Assert.IsType<Ticket>(okResult.Value);

            Assert.NotNull(createdTicket);
            Assert.Equal("VIP", createdTicket.Type);
            Assert.Equal(100, createdTicket.Price);
            Assert.Equal(10, createdTicket.QuantityAvailable);
            Assert.Equal(1, createdTicket.EventId);
        }

        [Fact]
        public async Task Create_SavesTicketToDatabase()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            var dto = new TicketCreateDto
            {
                EventId = 1,
                Type = "Regular",
                Price = 50,
                QuantityAvailable = 25
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdTicket = Assert.IsType<Ticket>(okResult.Value);

            // Verify in database
            var ticketInDb = await _context.Tickets.FindAsync(createdTicket.Id);
            Assert.NotNull(ticketInDb);
            Assert.Equal("Regular", ticketInDb.Type);
            Assert.Equal(50, ticketInDb.Price);
            Assert.Equal(1, ticketInDb.EventId);
        }

        [Fact]
        public async Task Create_ReturnsNotFound_WhenEventDoesNotExist()
        {
            // Arrange
            var controller = new TicketController(_context, _mapper);

            var dto = new TicketCreateDto
            {
                EventId = 999,  // Non-existent event
                Type = "VIP",
                Price = 100,
                QuantityAvailable = 10
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Event not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Create_AllowsMultipleTicketsForSameEvent()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            var dto1 = new TicketCreateDto
            {
                EventId = 1,
                Type = "VIP",
                Price = 100,
                QuantityAvailable = 10
            };

            var dto2 = new TicketCreateDto
            {
                EventId = 1,
                Type = "Regular",
                Price = 50,
                QuantityAvailable = 20
            };

            // Act
            var result1 = await controller.Create(dto1);
            var result2 = await controller.Create(dto2);

            // Assert
            var okResult1 = Assert.IsType<OkObjectResult>(result1);
            var okResult2 = Assert.IsType<OkObjectResult>(result2);

            var ticket1 = Assert.IsType<Ticket>(okResult1.Value);
            var ticket2 = Assert.IsType<Ticket>(okResult2.Value);

            var ticketsInDb = await _context.Tickets.Where(t => t.EventId == 1).ToListAsync();
            Assert.Equal(2, ticketsInDb.Count);
            Assert.Single(ticketsInDb.Where(t => t.Type == "VIP"));
            Assert.Single(ticketsInDb.Where(t => t.Type == "Regular"));
        }

        [Fact]
        public async Task Create_WithDifferentPrices()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            // Act - Create 3 different price tickets
            var prices = new decimal[] { 199.99m, 99.50m, 49.99m };
            foreach (var price in prices)
            {
                var dto = new TicketCreateDto
                {
                    EventId = 1,
                    Type = $"Type_{price}",
                    Price = price,
                    QuantityAvailable = 10
                };
                await controller.Create(dto);
            }

            // Assert
            var allTickets = await _context.Tickets.Where(t => t.EventId == 1).ToListAsync();
            Assert.Equal(3, allTickets.Count);
            Assert.Single(allTickets.Where(t => t.Price == 199.99m));
            Assert.Single(allTickets.Where(t => t.Price == 99.50m));
            Assert.Single(allTickets.Where(t => t.Price == 49.99m));
        }

        [Fact]
        public async Task Create_WithZeroQuantity()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            var dto = new TicketCreateDto
            {
                EventId = 1,
                Type = "SoldOut",
                Price = 0,
                QuantityAvailable = 0
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var ticket = Assert.IsType<Ticket>(okResult.Value);

            Assert.Equal(0, ticket.QuantityAvailable);
        }

        [Fact]
        public async Task Create_AssignsAutoIncrementId()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            var dto = new TicketCreateDto
            {
                EventId = 1,
                Type = "VIP",
                Price = 100,
                QuantityAvailable = 10
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var ticket = Assert.IsType<Ticket>(okResult.Value);

            Assert.True(ticket.Id > 0);
        }

        [Fact]
        public async Task Create_WithLargeQuantity()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            var dto = new TicketCreateDto
            {
                EventId = 1,
                Type = "Regular",
                Price = 50,
                QuantityAvailable = 999999
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var ticket = Assert.IsType<Ticket>(okResult.Value);

            Assert.Equal(999999, ticket.QuantityAvailable);
        }

        [Fact]
        public async Task Create_MapsFromDtoCorrectly()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            var dto = new TicketCreateDto
            {
                EventId = 1,
                Type = "TestType",
                Price = 123.45m,
                QuantityAvailable = 42
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdTicket = Assert.IsType<Ticket>(okResult.Value);

            Assert.Equal(dto.Type, createdTicket.Type);
            Assert.Equal(dto.Price, createdTicket.Price);
            Assert.Equal(dto.QuantityAvailable, createdTicket.QuantityAvailable);
            Assert.Equal(dto.EventId, createdTicket.EventId);
        }

        [Fact]
        public async Task Create_SequentialCreations_DifferentIds()
        {
            // Arrange
            var @event = new Event { Id = 1, Title = "Event", Location = "Baku" };
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            var dtos = new[]
            {
                new TicketCreateDto { EventId = 1, Type = "Type1", Price = 100, QuantityAvailable = 10 },
                new TicketCreateDto { EventId = 1, Type = "Type2", Price = 200, QuantityAvailable = 20 },
                new TicketCreateDto { EventId = 1, Type = "Type3", Price = 300, QuantityAvailable = 30 }
            };

            var createdIds = new List<int>();

            // Act
            foreach (var dto in dtos)
            {
                var result = await controller.Create(dto);
                var okResult = Assert.IsType<OkObjectResult>(result);
                var ticket = Assert.IsType<Ticket>(okResult.Value);
                createdIds.Add(ticket.Id);
            }

            // Assert
            Assert.Equal(3, createdIds.Count);
            // All IDs should be unique
            Assert.Equal(createdIds.Count, createdIds.Distinct().Count());
        }

        #endregion

        #region EDGE CASES & ERROR HANDLING

        [Fact]
        public async Task Create_ReturnsNotFound_WithMultipleInvalidEventIds()
        {
            // Arrange
            var controller = new TicketController(_context, _mapper);

            var invalidEventIds = new[] { 999, 888, 777 };

            // Act & Assert
            foreach (var eventId in invalidEventIds)
            {
                var dto = new TicketCreateDto
                {
                    EventId = eventId,
                    Type = "Test",
                    Price = 50,
                    QuantityAvailable = 10
                };

                var result = await controller.Create(dto);
                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("Event not found", notFoundResult.Value);
            }
        }

        [Fact]
        public async Task GetAll_WithMultipleEventsAndTickets()
        {
            // Arrange - Create complex scenario
            var events = new[]
            {
                new Event { Id = 1, Title = "Event 1", Location = "Baku" },
                new Event { Id = 2, Title = "Event 2", Location = "Ganja" },
                new Event { Id = 3, Title = "Event 3", Location = "Sumgait" }
            };

            foreach (var @event in events)
            {
                _context.Events.Add(@event);
            }

            var tickets = new[]
            {
                new Ticket { Id = 1, EventId = 1, Type = "VIP", Price = 100, QuantityAvailable = 5 },
                new Ticket { Id = 2, EventId = 1, Type = "Regular", Price = 50, QuantityAvailable = 20 },
                new Ticket { Id = 3, EventId = 2, Type = "Standard", Price = 75, QuantityAvailable = 30 },
                new Ticket { Id = 4, EventId = 3, Type = "Economy", Price = 25, QuantityAvailable = 100 }
            };

            foreach (var ticket in tickets)
            {
                _context.Tickets.Add(ticket);
            }
            await _context.SaveChangesAsync();

            var controller = new TicketController(_context, _mapper);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTickets = Assert.IsAssignableFrom<List<TicketReturnDto>>(okResult.Value);

            Assert.Equal(4, returnedTickets.Count);
            Assert.Equal(2, returnedTickets.FindAll(t => t.EventId == 1).Count);
            Assert.Single(returnedTickets.Where(t => t.EventId == 2));
            Assert.Single(returnedTickets.Where(t => t.EventId == 3));
        }

        #endregion
    }
}