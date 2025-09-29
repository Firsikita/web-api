using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [Produces("application/json", "application/xml")]
    [HttpGet("{userId}", Name = nameof(GetUserById))]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user == null)
            return NotFound();
        
        var userDto = _mapper.Map<UserDto>(user);
        return Ok(userDto);
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] UserToPostDto user)
    {
        var userToPost = _mapper.Map<UserEntity>(user);
        if (userToPost == null)
            return UnprocessableEntity();
        var value =  _userRepository.Insert(userToPost);
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = userToPost.Id },
            value);
    }
}