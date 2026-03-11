using BucketListAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;


var builder = WebApplication.CreateBuilder(args);
var connectionString = "Server=localhost;Database=BucketListDB;User=root;Password=1234;";
builder.Services.AddDbContext<BucketListDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));



// Add services to the container.
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

//hier komer onze get/set/post......
// GET: Alle bucket list items van een user (met executed status)
app.MapGet("/users/{userId}/bucketlist", async (int userId, BucketListDbContext db) =>
{
    var items = await db.Personalbucketlists
        .Where(pbl => pbl.FkUser == userId)
        .Include(pbl => pbl.FkBucketListItemNavigation)
        .Select(pbl => new
        {
            ItemId = pbl.FkBucketListItem,
            Name = pbl.FkBucketListItemNavigation.NameBucketListItem,
            Description = pbl.FkBucketListItemNavigation.DescriptionBucketListItem,
            Executed = pbl.Executed
        })
        .ToListAsync();

    return Results.Ok(items);
});

// POST: Voeg een item toe aan de bucket list van een user
app.MapPost("/users/{userId}/bucketlist/{itemId}", async (int userId, int itemId, BucketListDbContext db) =>
{
    // Check of item al bestaat voor deze user
    var exists = await db.Personalbucketlists
        .AnyAsync(pbl => pbl.FkUser == userId && pbl.FkBucketListItem == itemId);

    if (exists)
        return Results.Conflict("Item already in bucket list");

    var personalItem = new Personalbucketlist
    {
        FkUser = userId,
        FkBucketListItem = itemId,
        Executed = false
    };

    db.Personalbucketlists.Add(personalItem);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{userId}/bucketlist", personalItem);
});

// PUT: Markeer een item als executed/not executed
app.MapPut("/users/{userId}/bucketlist/{itemId}/toggle", async (int userId, int itemId, BucketListDbContext db) =>
{
    var item = await db.Personalbucketlists
        .FirstOrDefaultAsync(pbl => pbl.FkUser == userId && pbl.FkBucketListItem == itemId);

    if (item == null)
        return Results.NotFound();

    item.Executed = !item.Executed; // Toggle
    await db.SaveChangesAsync();

    return Results.Ok(new { Executed = item.Executed });
});

// DELETE: Verwijder een item uit de bucket list van een user
app.MapDelete("/users/{userId}/bucketlist/{itemId}", async (int userId, int itemId, BucketListDbContext db) =>
{
    var item = await db.Personalbucketlists
        .FirstOrDefaultAsync(pbl => pbl.FkUser == userId && pbl.FkBucketListItem == itemId);

    if (item == null)
        return Results.NotFound();

    db.Personalbucketlists.Remove(item);
    await db.SaveChangesAsync();

    return Results.NoContent();
});



// GET: Alle beschikbare bucket list items
app.MapGet("/bucketlistitems", async (BucketListDbContext db) =>
{
    var items = await db.Bucketlistitems
        .Select(pbl => new
        {
            ItemId = pbl.IdBucketListItem,
            Name = pbl.NameBucketListItem,
            Description = pbl.DescriptionBucketListItem
        }).ToListAsync();
    return Results.Ok(items);
});

// GET: Check username en password
app.MapGet("/users/login", async (string username, string password, BucketListDbContext db) =>
{
    var user = await db.Users
        .FirstOrDefaultAsync(u => u.NameUser == username && u.PassWordUser == password);

    if (user == null)
    return Results.Unauthorized();

    return Results.Ok(new
    {
        UserId = user.IdUser,
        Username = user.NameUser
    });
});
// POST: Voeg een nieuw bucket list item toe aan de databank
// POST: Voeg een item toe aan de bucket listitems
app.MapPost("/bucketlistitem", async (string itemName, string itemDescription, BucketListDbContext db) =>
{
    var exists = await db.Bucketlistitems
    .AnyAsync(pbl => pbl.NameBucketListItem == itemName);
    if (exists)
        return Results.Conflict("Item already in bucket list");
    var bucketlistitem = new Bucketlistitem
    {
        NameBucketListItem = itemName,
        DescriptionBucketListItem = itemDescription,
    };
    db.Bucketlistitems.Add(bucketlistitem);
    await db.SaveChangesAsync();

    return Results.Created($"bucketlistitem", bucketlistitem);
});
app.Run();