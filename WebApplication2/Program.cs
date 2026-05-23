using ApiProjectPractise;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.Services.AddServices(config);

// Add services to the container.
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Configure the HTTP request pipeline.

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();

app.Run();
