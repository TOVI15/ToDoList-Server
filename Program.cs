using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// שנה את השורה הזו מ- AddSingleton ל- AddDbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), ServerVersion.Parse("8.0.41-mysql")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", p => p
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader())
        );

builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(options => 
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// מיפוי נקודות קצה
app.MapGet("/", () => "This is a GET");

// קבלת כל הפריטים
app.MapGet("/items", (ToDoDbContext context) =>
{
    return context.Items.ToList();
});

// קבלת פריט לפי ID
app.MapGet("/items/{id}", async (int id, ToDoDbContext dbContext) =>
{
    var item = await dbContext.Items.FindAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

// הוספת פריט חדש
app.MapPost("/items", async (Item item, ToDoDbContext dbContext) =>
{
    dbContext.Items.Add(item);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

// עדכון פריט
// עדכון פריט
app.MapPut("/items/{id}", async (int id, Item item, ToDoDbContext dbContext) =>
{
    var i= await dbContext.Items.FindAsync(id);
    if(i==null)
        return Results.BadRequest("There is no such item!");
    i.IsComplete=item.IsComplete;
    await dbContext.SaveChangesAsync();
    return Results.Created($"/",i); 
});

// מחיקת פריט
app.MapDelete("/items/{id}", async (int id, ToDoDbContext dbContext) =>
{
    var item = await dbContext.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    dbContext.Items.Remove(item);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();