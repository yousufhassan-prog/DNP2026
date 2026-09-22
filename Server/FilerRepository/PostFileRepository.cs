using System.Text.Json;
using Entities;
using RepositoryContracts;
using System.IO;

namespace FilerRepository;

public class PostFileRepository : IPostRepository
{
    private readonly string filePath = "posts.json";

    public PostFileRepository()
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    private async Task<List<Post>> ReadAllPostsAsync()
    {
        string postsAsJson = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<Post>>(postsAsJson)!;
    }

    private async Task WriteAllPostsAsync(List<Post> posts)
    {
        string postsAsJson = JsonSerializer.Serialize(posts);
        await File.WriteAllTextAsync(filePath, postsAsJson);
    }

    public async Task<Post> AddAsync(Post post)
    {
        List<Post> posts = await ReadAllPostsAsync();
        post.Id = posts.Any() ? posts.Max(p => p.Id) + 1 : 1;
        posts.Add(post);
        await WriteAllPostsAsync(posts);
        return post;
    }

    public async Task UpdateAsync(Post post)
    {
        List<Post> posts = await ReadAllPostsAsync();
        Post? existingPost = posts.SingleOrDefault(p => p.Id == post.Id);
        if (existingPost is null) throw new InvalidOperationException($"Post with ID '{post.Id}' not found");
        
        posts.Remove(existingPost);
        posts.Add(post);
        await WriteAllPostsAsync(posts);
    }

    public async Task DeleteAsync(int id)
    {
        List<Post> posts = await ReadAllPostsAsync();
        Post? postToRemove = posts.SingleOrDefault(p => p.Id == id);
        if (postToRemove is null) throw new InvalidOperationException($"Post with ID '{id}' not found");
        
        posts.Remove(postToRemove);
        await WriteAllPostsAsync(posts);
    }

    public async Task<Post> GetSingleAsync(int id)
    {
        List<Post> posts = await ReadAllPostsAsync();
        Post? post = posts.SingleOrDefault(p => p.Id == id);
        if (post is null) throw new InvalidOperationException($"Post with ID '{id}' not found");
        
        return post;
    }

    public IQueryable<Post> GetMany()
    {
        string postsAsJson = File.ReadAllTextAsync(filePath).Result;
        List<Post> posts = JsonSerializer.Deserialize<List<Post>>(postsAsJson)!;
        return posts.AsQueryable();
    }
}