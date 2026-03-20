using BucketListAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

var builder = WebApplication.CreateBuilder(args);
var connectionString = "Server=localhost;Database=BucketListDB;User=root;Password=1234;";
builder.Services.AddDbContext<BucketListDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// GET: Alle bucket list items van een user
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

// PUT: Toggle executed
app.MapPut("/users/{userId}/bucketlist/{itemId}/toggle", async (int userId, int itemId, BucketListDbContext db) =>
{
    var item = await db.Personalbucketlists
        .FirstOrDefaultAsync(pbl => pbl.FkUser == userId && pbl.FkBucketListItem == itemId);

    if (item == null)
        return Results.NotFound();

    item.Executed = !item.Executed;
    await db.SaveChangesAsync();

    return Results.Ok(new { Executed = item.Executed });
});

// DELETE: Verwijder een item uit de persoonlijke bucket list
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

// GET: Login
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

// POST: Voeg een item toe aan de globale bucket list
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

// POST: Voeg een user toe
app.MapPost("/AddUser", async (string userName, string password, BucketListDbContext db) =>
{
    var exists = await db.Users.AnyAsync(pbl => pbl.NameUser == userName);

    if (exists)
        return Results.Conflict("User already in Database");

    var user = new User
    {
        NameUser = userName,
        PassWordUser = password
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"user", user);
});

// DELETE: Verwijder een item uit de globale bucketlistitems tabel
app.MapDelete("/bucketlistitem/{itemId}", async (int itemId, BucketListDbContext db) =>
{
    var item = await db.Bucketlistitems.FindAsync(itemId);

    if (item == null)
        return Results.NotFound();

    db.Bucketlistitems.Remove(item);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();