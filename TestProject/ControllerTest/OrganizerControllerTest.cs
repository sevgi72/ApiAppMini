using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebApplication2.Controllers;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Models;
using Xunit;

namespace TestProject.ControllerTests
{
    public class OrganizerControllerTests
    {
        private readonly ApiAppDbContext _context;
        private readonly IMapper _mapper;

        public OrganizerControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApiAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApiAppDbContext(options);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();

            context.Request.Host = new HostString("localhost:7000");
            context.Request.Scheme = "https";

            mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(context);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebApplication2.Profiles.MapperProfile(mockHttpContextAccessor.Object));
                cfg.ConstructServicesUsing(type => mockHttpContextAccessor.Object);
            });

            _mapper = config.CreateMapper();
        }

        #region GET ALL ORGANIZERS

        [Fact]
        public async Task GetAllOrganizers_ReturnsOkResult_WithList()
        {
            // Arrange
            _context.Organizers.Add(new Organizer
            {
                Id = 1,
                Name = "Test Organizer",
                Email = "test@gmail.com"
            });

            await _context.SaveChangesAsync();

            var controller = new OrganizerController(_context, _mapper);

            // Act
            var result = await controller.GetAllOrganizers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var organizers = Assert.IsAssignableFrom<List<OrganizerReturnDto>>(okResult.Value);
            Assert.Single(organizers);
        }

        [Fact]
        public async Task GetAllOrganizers_ReturnsOkResult_WithMultipleOrganizers()
        {
            // Arrange
            _context.Organizers.Add(new Organizer { Id = 1, Name = "Org 1", Email = "org1@gmail.com" });
            _context.Organizers.Add(new Organizer { Id = 2, Name = "Org 2", Email = "org2@gmail.com" });
            _context.Organizers.Add(new Organizer { Id = 3, Name = "Org 3", Email = "org3@gmail.com" });

            await _context.SaveChangesAsync();

            var controller = new OrganizerController(_context, _mapper);

            // Act
            var result = await controller.GetAllOrganizers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var organizers = Assert.IsAssignableFrom<List<OrganizerReturnDto>>(okResult.Value);
            Assert.Equal(3, organizers.Count);
        }

        [Fact]
        public async Task GetAllOrganizers_ReturnsOkResult_WithEmptyList()
        {
            // Arrange
            var controller = new OrganizerController(_context, _mapper);

            // Act
            var result = await controller.GetAllOrganizers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var organizers = Assert.IsAssignableFrom<List<OrganizerReturnDto>>(okResult.Value);
            Assert.Empty(organizers);
        }

        #endregion

        #region CREATE

        [Fact]
        public async Task Create_ReturnsOkResult_AndAddsOrganizer()
        {
            // Arrange
            var controller = new OrganizerController(_context, _mapper);

            var dto = new OrganizerCreateDto
            {
                Name = "New Organizer",
                Email = "new@gmail.com",
                Phone = "123456"
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var organizer = Assert.IsType<Organizer>(okResult.Value);

            Assert.Equal("New Organizer", organizer.Name);
            Assert.Equal("new@gmail.com", organizer.Email);

            var organizerInDb = await _context.Organizers.FindAsync(organizer.Id);
            Assert.NotNull(organizerInDb);
        }

        [Fact]
        public async Task Create_ReturnsOkResult_WithoutPhone()
        {
            // Arrange
            var controller = new OrganizerController(_context, _mapper);

            var dto = new OrganizerCreateDto
            {
                Name = "Organizer Without Phone",
                Email = "nophone@gmail.com"
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var organizer = Assert.IsType<Organizer>(okResult.Value);
            Assert.NotNull(organizer);
        }

        [Fact]
        public async Task Create_SavesOrganizerToDatabase()
        {
            // Arrange
            var controller = new OrganizerController(_context, _mapper);

            var dto = new OrganizerCreateDto
            {
                Name = "Database Test Org",
                Email = "dbtest@gmail.com"
            };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdOrganizer = Assert.IsType<Organizer>(okResult.Value);

            var organizerInDb = await _context.Organizers.FirstOrDefaultAsync(o => o.Email == "dbtest@gmail.com");
            Assert.NotNull(organizerInDb);
            Assert.Equal("Database Test Org", organizerInDb.Name);
        }

        #endregion

        #region UPLOAD LOGO

        [Fact]
        public async Task UploadLogo_ReturnsBadRequest_WhenFileIsNull()
        {
            // Arrange
            var controller = new OrganizerController(_context, _mapper);

            // Act
            var result = await controller.UploadLogo(1, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UploadLogo_ReturnsNotFound_WhenOrganizerNotExists()
        {
            // Arrange
            var controller = new OrganizerController(_context, _mapper);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.ContentType).Returns("image/png");

            // Act
            var result = await controller.UploadLogo(999, fileMock.Object);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UploadLogo_ReturnsBadRequest_WhenFileIsEmpty()
        {
            // Arrange
            _context.Organizers.Add(new Organizer { Id = 1, Name = "Test", Email = "test@gmail.com" });
            await _context.SaveChangesAsync();

            var controller = new OrganizerController(_context, _mapper);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await controller.UploadLogo(1, fileMock.Object);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region GET EVENTS BY ORGANIZER

        [Fact]
        public async Task GetEventsByOrganizer_ReturnsOkResult_WithEvents()
        {
            // Arrange
            _context.Organizers.Add(new Organizer
            {
                Id = 1,
                Name = "Main Organizer",
                Email = "main@gmail.com"
            });

            _context.Events.Add(new Event
            {
                Id = 1,
                Title = "Concert",
                Location = "Baku",
                OrganizerId = 1
            });

            await _context.SaveChangesAsync();

            var controller = new OrganizerController(_context, _mapper);

            // Act
            var result = await controller.GetEventsByOrganizer(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<List<EventReturnDto>>(okResult.Value);
            Assert.Single(events);
        }

        [Fact]
        public async Task GetEventsByOrganizer_ReturnsNotFound_WhenOrganizerNotExists()
        {
            // Arrange
            var controller = new OrganizerController(_context, _mapper);

            // Act
            var result = await controller.GetEventsByOrganizer(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetEventsByOrganizer_ReturnsEmptyList_WhenOrganizerHasNoEvents()
        {
            // Arrange
            _context.Organizers.Add(new Organizer { Id = 1, Name = "Org", Email = "org@gmail.com" });
            await _context.SaveChangesAsync();

            var controller = new OrganizerController(_context, _mapper);

            // Act
            var result = await controller.GetEventsByOrganizer(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<List<EventReturnDto>>(okResult.Value);
            Assert.Empty(events);
        }

        [Fact]
        public async Task GetEventsByOrganizer_ReturnsMultipleEvents()
        {
            // Arrange
            _context.Organizers.Add(new Organizer { Id = 1, Name = "Event Org", Email = "events@gmail.com" });
            _context.Events.Add(new Event { Id = 1, Title = "Event 1", Location = "Baku", OrganizerId = 1 });
            _context.Events.Add(new Event { Id = 2, Title = "Event 2", Location = "Ganja", OrganizerId = 1 });
            _context.Events.Add(new Event { Id = 3, Title = "Event 3", Location = "Sumgait", OrganizerId = 1 });

            await _context.SaveChangesAsync();

            var controller = new OrganizerController(_context, _mapper);

            // Act
            var result = await controller.GetEventsByOrganizer(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<List<EventReturnDto>>(okResult.Value);
            Assert.Equal(3, events.Count);
        }

        [Fact]
        public async Task GetEventsByOrganizer_ReturnsOnlyOrganizerEvents()
        {
            // Arrange
            _context.Organizers.Add(new Organizer { Id = 1, Name = "Org 1", Email = "org1@gmail.com" });
            _context.Organizers.Add(new Organizer { Id = 2, Name = "Org 2", Email = "org2@gmail.com" });

            _context.Events.Add(new Event { Id = 1, Title = "Org1 Event", Location = "Baku", OrganizerId = 1 });
            _context.Events.Add(new Event { Id = 2, Title = "Org2 Event", Location = "Ganja", OrganizerId = 2 });

            await _context.SaveChangesAsync();

            var controller = new OrganizerController(_context, _mapper);

            // Act
            var result = await controller.GetEventsByOrganizer(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<List<EventReturnDto>>(okResult.Value);
            Assert.Single(events);
            Assert.Equal("Org1 Event", events[0].Title);
        }

        #endregion
    }
}