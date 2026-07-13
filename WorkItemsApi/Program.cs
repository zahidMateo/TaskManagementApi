using Microsoft.EntityFrameworkCore;
using WorkItemsApi.Data;
using WorkItemsApi.Integration;
using WorkItemsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        var errors = string.Join(" | ", context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage));
        logger.LogWarning("Validation failed for request '{Path}': {Errors}", context.HttpContext.Request.Path, errors);
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(context.ModelState);
    };
});

// Add SQLite Database
builder.Services.AddDbContext<WorkItemsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=workitems.db"));

// Register Typed HttpClient for UserManagementApi
builder.Services.AddHttpClient<IUserManagementClient, UserManagementClient>(client =>
{
    var baseUrl = builder.Configuration["UserManagementApi:BaseUrl"] ?? "http://localhost:5001/";
    client.BaseAddress = new Uri(baseUrl);
});

// Register Services
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();

// Configure CORS to allow frontend connections
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Automatically create and seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<WorkItemsDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
