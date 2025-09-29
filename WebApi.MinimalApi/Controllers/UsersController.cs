using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json", "application/xml")]
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
        if (user == null)
            return BadRequest();
        if (string.IsNullOrEmpty(user.Login) || !user.Login.All(char.IsLetterOrDigit))
            ModelState.AddModelError(nameof(user.Login), "Unallowed chars in Login");
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        
        var userToPost = _mapper.Map<UserEntity>(user);
        
        var value =  _userRepository.Insert(userToPost);
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = userToPost.Id },
            value.Id);
    }
    
    [HttpPut("{userId}")]
    public IActionResult UpdateUser([FromRoute] Guid userId, [FromBody] UserUpdateDto? userUpdateDto)
    {
        if (userUpdateDto == null || userId == Guid.Empty)
            return BadRequest();

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        var updatedUser = _mapper.Map(userUpdateDto, new UserEntity(userId));

        _userRepository.UpdateOrInsert(updatedUser, out var isNewUser);
    
        if (isNewUser)
        {
            return CreatedAtAction(
                actionName: nameof(GetUserById),
                routeValues: new { userId = updatedUser.Id },
                value: updatedUser.Id);
        }

        return NoContent();
    }
}