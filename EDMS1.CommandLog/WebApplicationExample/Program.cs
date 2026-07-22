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
builder.Services.AddDbContext<CommandLogDbContext>(o => o.UseInMemoryDatabase("example"));
// Алиас, чтобы нормально получать AppDbContext при такой цепочке наследования.
builder.Services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<CommandLogDbContext>());

builder.Services.AddCommandLogService<CommandLogDbContext>(typeof(AddTodo));

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
