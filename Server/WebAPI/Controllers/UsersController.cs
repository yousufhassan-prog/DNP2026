using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;
using System.Linq;

namespace WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository userRepo;

    public UsersController(IUserRepository userRepo)
    {
        this.userRepo = userRepo;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto request)
    {
        if (userRepo.GetMany().Any(u => u.Username.Equals(request.UserName, StringComparison.OrdinalIgnoreCase)))
            return BadRequest($"Username '{request.UserName}' is already taken.");

        User user = new User { Username = request.UserName, Password = request.Password };
        User created = await userRepo.AddAsync(user);

        return Created($"/Users/{created.Id}", new UserDto { Id = created.Id, UserName = created.Username });
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetMany([FromQuery] string? nameContains)
    {
        var query = userRepo.GetMany();

        if (!string.IsNullOrWhiteSpace(nameContains))
            query = query.Where(u => u.Username.Contains(nameContains, StringComparison.OrdinalIgnoreCase));

        return Ok(query.Select(u => new UserDto { Id = u.Id, UserName = u.Username }).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetSingle([FromRoute] int id)
    {
        User u = await userRepo.GetSingleAsync(id);
        return Ok(new UserDto { Id = u.Id, UserName = u.Username });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update([FromRoute] int id, [FromBody] CreateUserDto request)
    {
        await userRepo.UpdateAsync(new User { Id = id, Username = request.UserName, Password = request.Password });
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] int id)
    {
        await userRepo.DeleteAsync(id);
        return Ok();
    }
}