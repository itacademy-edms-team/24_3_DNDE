using EDMS1.CommandLog.Extensions;
using Microsoft.EntityFrameworkCore;
using WebApplicationExample.Commands;
using WebApplicationExample.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрируем самый нижний узел в дереве наследования.
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("example"));
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

builder.Services.AddCommandLogService<AppDbContext>(typeof(AddTodo));

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(AddTodo).Assembly);
    config.AddCommandLogBehavior();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
