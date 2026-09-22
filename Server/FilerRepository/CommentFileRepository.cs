using System.Text.Json;
using Entities;
using RepositoryContracts;
using System.IO;

namespace FilerRepository;

public class CommentFileRepository : ICommentRepository
{
    private readonly string filePath = "comments.json";

    public CommentFileRepository()
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    private async Task<List<Comment>> ReadAllCommentsAsync()
    {
        string commentsAsJson = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<Comment>>(commentsAsJson)!;
    }

    private async Task WriteAllCommentsAsync(List<Comment> comments)
    {
        string commentsAsJson = JsonSerializer.Serialize(comments);
        await File.WriteAllTextAsync(filePath, commentsAsJson);
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        List<Comment> comments = await ReadAllCommentsAsync();
        comment.Id = comments.Any() ? comments.Max(c => c.Id) + 1 : 1;
        comments.Add(comment);
        await WriteAllCommentsAsync(comments);
        return comment;
    }

    public async Task UpdateAsync(Comment comment)
    {
        List<Comment> comments = await ReadAllCommentsAsync();
        Comment? existingComment = comments.SingleOrDefault(c => c.Id == comment.Id);
        if (existingComment is null) throw new InvalidOperationException($"Comment with ID '{comment.Id}' not found");
        
        comments.Remove(existingComment);
        comments.Add(comment);
        await WriteAllCommentsAsync(comments);
    }

    public async Task DeleteAsync(int id)
    {
        List<Comment> comments = await ReadAllCommentsAsync();
        Comment? commentToRemove = comments.SingleOrDefault(c => c.Id == id);
        if (commentToRemove is null) throw new InvalidOperationException($"Comment with ID '{id}' not found");
        
        comments.Remove(commentToRemove);
        await WriteAllCommentsAsync(comments);
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        List<Comment> comments = await ReadAllCommentsAsync();
        Comment? comment = comments.SingleOrDefault(c => c.Id == id);
        if (comment is null) throw new InvalidOperationException($"Comment with ID '{id}' not found");
        
        return comment;
    }

    public IQueryable<Comment> GetMany()
    {
        string commentsAsJson = File.ReadAllTextAsync(filePath).Result;
        List<Comment> comments = JsonSerializer.Deserialize<List<Comment>>(commentsAsJson)!;
        return comments.AsQueryable();
    }
}