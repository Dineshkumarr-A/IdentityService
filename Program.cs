using IdentityService.Data;
using IdentityService.Extensions;
using IdentityService.Filters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseProvider(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthService(builder.Configuration);

builder.Services.AddControllers(options =>
{
    //Aggregate the model validations
    options.Filters.Add<ValidateModelFilter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
