using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(DataContext context)
{
    if (await context.Users.AnyAsync()) return;

    Console.WriteLine("Reading user data...");
    var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

    if (users == null) 
    {
        Console.WriteLine("No users found to seed.");
        return;
    }

    foreach (var user in users)
    {
        using var hmac = new HMACSHA512();
        user.UserName = user.UserName.ToLower();
        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("pa$$w0rd"));
        user.PasswordSalt = hmac.Key;

        Console.WriteLine($"Adding user: {user.UserName}");
        context.Users.Add(user);
    }

    Console.WriteLine("Saving changes to the database...");
    await context.SaveChangesAsync();
    Console.WriteLine("Database seeded successfully.");
}

}
