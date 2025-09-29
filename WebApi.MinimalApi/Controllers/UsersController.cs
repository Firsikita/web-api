using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;

    public UsersController(IUserRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("users/{userId}")]
    [HttpGet("api/users/{userId}")]
    public IActionResult GetUserById(string userId)
    {
        if (!Guid.TryParse(userId, out var id))
            return NotFoundNoContentType();

        var entity = _repo.FindById(id);
        if (entity is null)
            return NotFoundNoContentType();

        var dto = new UserDto
        {
            Login = entity.Login,
            Id = entity.Id,
            FullName = $"{entity.LastName} {entity.FirstName}",
            GamesPlayed = entity.GamesPlayed,
            CurrentGameId = entity.CurrentGameId
        };

        return Ok(dto);
    }

    [HttpHead("users/{userId}")]
    [HttpHead("api/users/{userId}")]
    public IActionResult HeadUserById(string userId)
    {
        if (!Guid.TryParse(userId, out var id))
            return NotFoundNoContentType();

        var exists = _repo.FindById(id) is not null;
        if (!exists)
            return NotFoundNoContentType();

        Response.ContentType = "application/json; charset=utf-8";
        return Ok();
    }

    [HttpDelete("users/{userId}")]
    [HttpDelete("api/users/{userId}")]
    public IActionResult DeleteUser(string userId)
    {
        if (!Guid.TryParse(userId, out var id))
            return NotFoundNoContentType();

        var entity = _repo.FindById(id);
        if (entity is null)
            return NotFoundNoContentType();

        _repo.Delete(id);
        return NoContent();
    }

    [HttpPost("users")]
    [HttpPost("api/users")]
    [Produces("application/json")]
    public IActionResult CreateUser([FromBody] CreateUserRequest? body)
    {
        if (body is null)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(body.Login))
            return UnprocessableEntity(new Dictionary<string, string> { { "login", "Login is required." } });

        var toInsert = new UserEntity
        {
            Login = body.Login!,
            LastName = string.IsNullOrWhiteSpace(body.LastName) ? "Doe" : body.LastName!,
            FirstName = string.IsNullOrWhiteSpace(body.FirstName) ? "John" : body.FirstName!,
            GamesPlayed = 0,
            CurrentGameId = null
        };

        var created = _repo.Insert(toInsert);

        var location = $"{Request.Scheme}://{Request.Host}/api/users/{created.Id}";
        return Created(location, created.Id);
    }

    [HttpGet("users")]
    [HttpGet("api/users")]
    [Produces("application/json")]
    public IActionResult GetUsers([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
    {
        var page = pageNumber.GetValueOrDefault(1);
        var size = pageSize.GetValueOrDefault(10);

        if (page < 1) page = 1;
        if (size < 1) size = 1;
        if (size > 20) size = 20;

        var pageList = _repo.GetPage(page, size);

        string basePath = "/api/users";
        string? prev = pageList.HasPrevious ? $"{basePath}?pageNumber={page - 1}&pageSize={size}" : null;
        string? next = pageList.HasNext ? $"{basePath}?pageNumber={page + 1}&pageSize={size}" : null;

        var paginationHeader = new
        {
            PreviousPageLink = prev,
            NextPageLink = next,
            TotalCount = pageList.TotalCount,
            PageSize = pageList.PageSize,
            CurrentPage = pageList.CurrentPage,
            TotalPages = pageList.TotalPages
        };

        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(paginationHeader);

        var users = pageList.Select(u => new UserDto
        {
            Login = u.Login,
            Id = u.Id,
            FullName = $"{u.LastName} {u.FirstName}",
            GamesPlayed = u.GamesPlayed,
            CurrentGameId = u.CurrentGameId
        });

        return Ok(users);
    }

    private StatusCodeResult NotFoundNoContentType()
    {
        Response.Headers.Remove("Content-Type");
        return StatusCode(StatusCodes.Status404NotFound);
    }
}
