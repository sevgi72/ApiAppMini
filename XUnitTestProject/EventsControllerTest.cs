using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Controllers;
using WebApplication2.Data;
using WebApplication2.Models;
using Xunit;

namespace XUnitTestProject
{
    public class EventsControllerTests
    {
        private readonly ApiAppDbContext _context;
        private readonly IMapper _mapper;

        public EventsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApiAppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApiAppDbContext(options);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(Program).Assembly);
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task GetAllEvents_ReturnsOkResult()
        {
            // Arrange

            _context.Events.Add(new Event
            {
                Title = "Test Event",
                Description = "Test Desc",
                Location = "Baku",
                Date = DateTime.Now.AddDays(1)
            });

            await _context.SaveChangesAsync();

            var controller = new EventsController(_context, _mapper);

            // Act

            var result = await controller.GetAllEvents();

            // Assert

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
