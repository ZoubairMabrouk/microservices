using MdxServices.Interfaces;
using MdxServices.MDX;
using MdxServices.Middleware;
using MdxServices.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Mdx Services API",
        Version = "v1",
        Description = "OLAP Consumer for Document Analysis Axes"
    });
});
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())
);

builder.Services.AddScoped<IMdxService, MdxService>();
builder.Services.AddScoped<IMdxQuery, MdxQuery>();

var app = builder.Build();

app.UseMiddleware<MdxExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
