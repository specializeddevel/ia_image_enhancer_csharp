using ImageProcessor.Core;
using ImageProcessor.Api;
using Microsoft.OpenApi.Models; // Add this using directive
using Swashbuckle.AspNetCore.SwaggerGen; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Enables the use of controllers

// Register the ImageProcessorService as a singleton for dependency injection.
// This means a single instance will be created and shared across all HTTP requests.
builder.Services.AddSingleton<ImageProcessorService>();
builder.Services.AddSingleton<JobService>();
builder.Services.AddSingleton<ProcessingLogService>();

// Add OpenAPI/Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ImageProcessor API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// This maps the routes defined in our controller classes.
app.MapControllers();

app.Run();
