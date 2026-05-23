using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication2.Data;
using WebApplication2.Profiles;

namespace ApiProjectPractise
{
    public static class ServiceRegistration
    {
        public static void AddServices(this IServiceCollection services,IConfiguration config)
        {
            services.AddControllers();
            services.AddDbContext<ApiAppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddHttpContextAccessor();
            services.AddAutoMapper(cfg => cfg.AddProfile<MapperProfile>());
        }
    }
}
