using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;
using System.Linq;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository commentRepo;
    private readonly IUserRepository userRepo;

    public CommentsController(ICommentRepository commentRepo, IUserRepository userRepo)
    {
        this.commentRepo = commentRepo;
        this.userRepo = userRepo;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto request)
    {
        _ = await userRepo.GetSingleAsync(request.UserId);
        Comment comment = new Comment { Body = request.Body, PostId = request.PostId, UserId = request.UserId };
        Comment created = await commentRepo.AddAsync(comment);
        
        return Created($"/Comments/{created.Id}", new CommentDto { Id = created.Id, Body = created.Body, PostId = created.PostId, UserId = created.UserId });
    }

    [HttpGet]
    public ActionResult<IEnumerable<CommentDto>> GetMany([FromQuery] int? postId, [FromQuery] int? userId, [FromQuery] string? userName)
    {
        var query = commentRepo.GetMany();

        if (postId.HasValue)
            query = query.Where(c => c.PostId == postId.Value);
        
        if (userId.HasValue)
            query = query.Where(c => c.UserId == userId.Value);
        
        if (!string.IsNullOrWhiteSpace(userName))
        {
            var userIds = userRepo.GetMany().Where(u => u.Username.Contains(userName, StringComparison.OrdinalIgnoreCase)).Select(u => u.Id).ToList();
            query = query.Where(c => userIds.Contains(c.UserId));
        }

        return Ok(query.Select(c => new CommentDto { Id = c.Id, Body = c.Body, PostId = c.PostId, UserId = c.UserId }).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetSingle([FromRoute] int id)
    {
        Comment c = await commentRepo.GetSingleAsync(id);
        return Ok(new CommentDto { Id = c.Id, Body = c.Body, PostId = c.PostId, UserId = c.UserId });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] CreateCommentDto request)
    {
        await commentRepo.UpdateAsync(new Comment { Id = id, Body = request.Body, PostId = request.PostId, UserId = request.UserId });
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await commentRepo.DeleteAsync(id);
        return Ok();
    }
}