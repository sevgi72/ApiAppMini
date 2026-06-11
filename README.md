# 🎫 Event Management API - WebApplication

A comprehensive ASP.NET Core Web API for managing events, organizers, and tickets with full test coverage.

---

## 📋 Project Overview

**WebApplication2** is a complete event management system with:
- Event creation and management
- Organizer management
- Ticket management for events
- File uploads (event banners, organizer logos)
- Comprehensive unit test suite

---

 🏗️ Solution Structure

```
WebApplication2.sln
├── WebApplication2/           (Main API Project)
│   ├── Controllers/
│   │   ├── EventsController.cs
│   │   ├── OrganizerController.cs
│   │   └── TicketController.cs
│   ├── Models/
│   │   ├── Event.cs
│   │   ├── Organizer.cs
│   │   └── Ticket.cs
│   ├── Dtos/
│   │   ├── EventDtos/
│   │   ├── OrganizerDtos/
│   │   └── TicketDtos/
│   ├── Data/
│   │   └── ApiAppDbContext.cs
│   ├── Profiles/
│   │   └── MapperProfile.cs
│   ├── Extensions/
│   ├── wwwroot/
│   │   └── images/           (Uploaded files)
│   └── Program.cs
│
└── TestProject/              (Test Project)
    ├── ControllerTests/
    │   ├── EventsControllerTests.cs
    │   ├── OrganizerControllerTests.cs
    │   └── TicketControllerTests.cs
    └── TestProject.csproj
```

---

## 🔧 Technologies & Stack

### Backend
- **Framework:** ASP.NET Core 8.0+
- **Database:** Microsoft SQL Server (Entity Framework Core)
- **API:** RESTful Web API
- **ORM:** Entity Framework Core

### Data & Mapping
- **AutoMapper:** DTO mapping
- **Fluent Validation:** Input validation
- **DTOs:** Data Transfer Objects for API contracts

### Testing
- **xUnit:** Unit testing framework
- **Moq:** Mocking library
- **InMemory Database:** Test database

### Security & File Handling
- **File Upload:** Image uploads (banners & logos)
- **Image Validation:** File type and size checks

---

## 📊 Database Schema

### Event Entity
```csharp
public class Event
{
    public int Id { get; set; }
    public string Title { get; set; }           // Required, max 150 chars
    public string Description { get; set; }     // Optional, max 500 chars
    public DateTime Date { get; set; }          // Required, must be future date
    public string Location { get; set; }        // Required, max 200 chars
    public string BannerImageUrl { get; set; }  // Path to uploaded image
    public int OrganizerId { get; set; }        // Foreign Key
    public Organizer Organizer { get; set; }    // Navigation property
    public ICollection<Ticket> Tickets { get; set; }
}
```

### Organizer Entity
```csharp
public class Organizer
{
    public int Id { get; set; }
    public string Name { get; set; }            // Required, max 100 chars
    public string Email { get; set; }           // Required, valid email
    public string Phone { get; set; }           // Optional, max 20 chars
    public string LogoUrl { get; set; }         // Path to uploaded logo
    public ICollection<Event> Events { get; set; }
}
```

### Ticket Entity
```csharp
public class Ticket
{
    public int Id { get; set; }
    public int EventId { get; set; }            // Foreign Key
    public string Type { get; set; }            // Required, e.g. "VIP", "Regular"
    public decimal Price { get; set; }          // Required, positive
    public int QuantityAvailable { get; set; }  // Required, non-negative
    public Event Event { get; set; }            // Navigation property
}
```

---

## 🚀 API Endpoints

### Events
```
GET    /api/events                          Get all events
POST   /api/events                          Create new event
GET    /api/events/{id}                     Get event by ID
PUT    /api/events/{id}                     Update event
DELETE /api/events/{id}                     Delete event
POST   /api/events/{id}/banner              Upload event banner
GET    /api/events/{eventId}/tickets        Get tickets for event
GET    /api/events/{eventId}/organizer      Get organizer for event
POST   /api/events/{eventId}/tickets        Create ticket for event
```

### Organizers
```
GET    /api/organizers                      Get all organizers
POST   /api/organizers                      Create organizer
POST   /api/organizers/{id}/logo            Upload organizer logo
GET    /api/organizers/{organizerId}/events Get events by organizer
```

### Tickets
```
GET    /api/ticket                          Get all tickets
POST   /api/ticket                          Create ticket
```

---

## 🧪 Testing

### Test Coverage

| Controller | Tests | Coverage |
|------------|-------|----------|
| EventsController | ✅ Provided | Complete CRUD + Relations |
| OrganizerController | ✅ 17 tests | GetAll, Create, Upload, GetEvents |
| TicketController | ✅ 20 tests | GetAll, Create, Validations |
| **Total** | **37+ tests** | Comprehensive |

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific controller tests
dotnet test --filter "FullyQualifiedName~OrganizerControllerTests"
dotnet test --filter "FullyQualifiedName~TicketControllerTests"

# Run specific test
dotnet test --filter "Name=Create_ReturnsOkResult_WithValidDto"

# Run with verbose output
dotnet test -v detailed
```

### Test Files
- `EventsControllerTests.cs` - Event management tests
- `OrganizerControllerTests.cs` - Organizer management tests
- `TicketControllerTests.cs` - Ticket management tests

---

## 📥 Installation & Setup

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 (or Visual Studio Code)
- SQL Server LocalDB or SQL Server instance

### Clone & Setup
```bash
# Clone the repository
git clone <repository-url>

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Update database (if migrations exist)
dotnet ef database update
```

### Running the Application
```bash
# Navigate to WebApplication2
cd WebApplication2

# Run the application
dotnet run

# Application runs on https://localhost:5271 by default
```

### Testing
```bash
# Navigate to TestProject
cd TestProject

# Run tests
dotnet test
```

---

## 🔐 Configuration

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EventManagementDB;User Id=sa;Password=YourPassword;"
  },
  "AllowedHosts": "*"
}
```

### File Upload Configuration
- **Location:** `wwwroot/images/`
- **Max Size (Banner):** 2MB
- **Max Size (Logo):** 5MB
- **Allowed Types:** PNG, JPG, JPEG

---

## 🎯 Key Features

✅ **Event Management**
- Create, read, update, delete events
- Upload event banners
- Track event dates and locations

✅ **Organizer Management**
- Manage event organizers
- Upload organizer logos
- Associate events with organizers

✅ **Ticket Management**
- Create different ticket types per event
- Set ticket prices and quantities
- Manage ticket availability

✅ **Data Validation**
- FluentValidation for all inputs
- Future date validation for events
- Email format validation
- File type and size validation

✅ **API Best Practices**
- RESTful endpoints
- Proper HTTP status codes
- Error handling
- DTO pattern

✅ **Testing**
- Comprehensive unit tests
- InMemory database for testing
- Mock dependencies
- Edge case coverage

---

## 📝 Validation Rules

### Event
- Title: Required, max 150 characters
- Date: Required, must be future date
- Location: Required, max 200 characters
- Description: Optional, max 500 characters

### Organizer
- Name: Required, max 100 characters
- Email: Required, valid email format
- Phone: Optional, max 20 characters

### Ticket
- Type: Required, max 50 characters
- Price: Required, must be positive
- Quantity: Required, non-negative
- Event: Must exist in database

---

## 🔄 Database Relationships

```
Organizer (1) ──── (Many) Event
Event (1) ──── (Many) Ticket
```

**Cascade Delete:** When an event is deleted, related tickets are automatically deleted.

---

## 🛠️ Development Guidelines

### Adding New Features
1. Create model in `Models/`
2. Add DbSet to `ApiAppDbContext`
3. Create DTOs in `Dtos/`
4. Add AutoMapper profile in `Profiles/`
5. Create controller in `Controllers/`
6. Write tests in `TestProject/`

### Code Structure
- **Models:** Entity Framework entities
- **DTOs:** Data Transfer Objects for API contracts
- **Controllers:** API endpoints
- **Dtos:** Request/response objects
- **Data:** Database context and configurations
- **Profiles:** AutoMapper configurations

---

## 🐛 Troubleshooting

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure database exists

### File Upload Issues
- Check `wwwroot/images/` directory exists
- Verify file size limits
- Check allowed file types

### Test Failures
- Clear bin/obj folders: `dotnet clean`
- Restore packages: `dotnet restore`
- Run: `dotnet test -v detailed`

---

## 📚 Documentation Files

- **TEST_DOCUMENTATION.md** - Complete testing guide
- **QUICK_REFERENCE.md** - Quick reference for testing
- **ORGANIZER_TESTS_LIST.md** - OrganizerController test list
- **TICKET_CONTROLLER_TEST_SUMMARY.md** - TicketController test details

---

## 🚀 Deployment

### Build for Production
```bash
dotnet publish -c Release -o ./publish
```

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish .
ENTRYPOINT ["dotnet", "WebApplication2.dll"]
```

---

## 📞 Support & Contributions

For issues or contributions:
1. Create an issue with detailed description
2. Fork the repository
3. Create a feature branch
4. Submit a pull request

---

## 📄 License

This project is provided as-is for educational and development purposes.

---

## 🎓 Learning Resources

### ASP.NET Core
- [Official Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

### Testing
- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore)
- [Moq Documentation](https://github.com/moq/moq4/wiki/Quickstart)
- [AutoMapper Testing](https://docs.automapper.org/en/stable/Testing.html)

### API Design
- [RESTful API Best Practices](https://restfulapi.net/)
- [HTTP Status Codes](https://httpwg.org/specs/rfc7231.html#status.codes)

---

## 🔄 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | May 26, 2026 | Initial release with comprehensive test suite |

---

## ✅ Checklist for New Developers

- [ ] Clone repository
- [ ] Install .NET 8.0 SDK
- [ ] Restore NuGet packages: `dotnet restore`
- [ ] Update database connection in `appsettings.json`
- [ ] Run migrations: `dotnet ef database update`
- [ ] Run tests: `dotnet test`
- [ ] Start application: `dotnet run`
- [ ] Test API endpoints in Swagger UI

---

**Last Updated:** May 26, 2026  
**Maintainer:** Development Team  
**Status:** ✅ Active Development

---

## 📊 Project Statistics

```
Solution Projects:        2
├── Main API Project:      1
└── Test Project:          1

Total Controllers:        3
├── EventsController:      1
├── OrganizerController:   1
└── TicketController:      1

Total Test Methods:      37+
├── EventsControllerTests:       (provided)
├── OrganizerControllerTests:    17 tests
└── TicketControllerTests:       20 tests

API Endpoints:           14+
Database Entities:        3
DTOs:                    9
```
Deployed...
