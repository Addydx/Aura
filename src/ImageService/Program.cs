using ImageService.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Image Service API", 
        Version = "v1",
        Description = "API para gestión de imágenes con Cloudinary y MongoDB"
    });
});

// Custom services - register as Scoped for better lifecycle management
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<MongoService>();
builder.Services.AddScoped<RabbitMQService>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
