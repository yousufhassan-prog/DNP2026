using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;
using System.Linq;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostRepository postRepo;
    private readonly IUserRepository userRepo;

    public PostsController(IPostRepository postRepo, IUserRepository userRepo)
    {
        this.postRepo = postRepo;
        this.userRepo = userRepo;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost([FromBody] CreatePostDto request)
    {
        _ = await userRepo.GetSingleAsync(request.UserId);
        Post post = new Post { Title = request.Title, Body = request.Body, UserId = request.UserId };
        Post created = await postRepo.AddAsync(post);
        
        return Created($"/Posts/{created.Id}", new PostDto { Id = created.Id, Title = created.Title, Body = created.Body, UserId = created.UserId });
    }

    [HttpGet]
    public ActionResult<IEnumerable<PostDto>> GetMany([FromQuery] string? titleContains, [FromQuery] int? userId, [FromQuery] string? userName)
    {
        var query = postRepo.GetMany();

        if (!string.IsNullOrWhiteSpace(titleContains))
            query = query.Where(p => p.Title.Contains(titleContains, StringComparison.OrdinalIgnoreCase));
        
        if (userId.HasValue)
            query = query.Where(p => p.UserId == userId.Value);
        
        if (!string.IsNullOrWhiteSpace(userName))
        {
            var userIds = userRepo.GetMany().Where(u => u.Username.Contains(userName, StringComparison.OrdinalIgnoreCase)).Select(u => u.Id).ToList();
            query = query.Where(p => userIds.Contains(p.UserId));
        }

        return Ok(query.Select(p => new PostDto { Id = p.Id, Title = p.Title, Body = p.Body, UserId = p.UserId }).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetSingle([FromRoute] int id)
    {
        Post p = await postRepo.GetSingleAsync(id);
        return Ok(new PostDto { Id = p.Id, Title = p.Title, Body = p.Body, UserId = p.UserId });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] CreatePostDto request)
    {
        await postRepo.UpdateAsync(new Post { Id = id, Title = request.Title, Body = request.Body, UserId = request.UserId });
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await postRepo.DeleteAsync(id);
        return Ok();
    }
}