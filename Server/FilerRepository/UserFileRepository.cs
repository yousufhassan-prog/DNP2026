using System.Text.Json;
using Entities;
using RepositoryContracts;
using System.IO;
namespace FilerRepository;

public class UserFileRepository : IUserRepository
{
    private readonly string filePath = "users.json";

    public UserFileRepository()
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    private async Task<List<User>> ReadAllUsersAsync()
    {
        string usersAsJson = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<User>>(usersAsJson)!;
    }

    private async Task WriteAllUsersAsync(List<User> users)
    {
        string usersAsJson = JsonSerializer.Serialize(users);
        await File.WriteAllTextAsync(filePath, usersAsJson);
    }

    public async Task<User> AddAsync(User user)
    {
        List<User> users = await ReadAllUsersAsync();
        user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
        users.Add(user);
        await WriteAllUsersAsync(users);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        List<User> users = await ReadAllUsersAsync();
        User? existingUser = users.SingleOrDefault(u => u.Id == user.Id);
        if (existingUser is null) throw new InvalidOperationException($"User with ID '{user.Id}' not found");
        
        users.Remove(existingUser);
        users.Add(user);
        await WriteAllUsersAsync(users);
    }

    public async Task DeleteAsync(int id)
    {
        List<User> users = await ReadAllUsersAsync();
        User? userToRemove = users.SingleOrDefault(u => u.Id == id);
        if (userToRemove is null) throw new InvalidOperationException($"User with ID '{id}' not found");
        
        users.Remove(userToRemove);
        await WriteAllUsersAsync(users);
    }

    public async Task<User> GetSingleAsync(int id)
    {
        List<User> users = await ReadAllUsersAsync();
        User? user = users.SingleOrDefault(u => u.Id == id);
        if (user is null) throw new InvalidOperationException($"User with ID '{id}' not found");
        
        return user;
    }

    public IQueryable<User> GetMany()
    {
        string usersAsJson = File.ReadAllTextAsync(filePath).Result;
        List<User> users = JsonSerializer.Deserialize<List<User>>(usersAsJson)!;
        return users.AsQueryable();
    }
}