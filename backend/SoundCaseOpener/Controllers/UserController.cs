using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Core.Services;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;
using SoundCaseOpener.Util;

namespace SoundCaseOpener.Controllers;

[Route("api/users")]
public class UserController(IUserService userService,
                            ITransactionProvider transaction,
                            ILogger<UserController> logger) : BaseController
{
    [HttpGet]
    [Route("username/{username}")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<UserDto>> GetUserByUsername([FromRoute] string username) =>
        (await userService.GetUserByUsername(username)).Match<ActionResult<UserDto>>(
             user => Ok(UserDto.FromUser(user)),
             notFound => NotFound());
    
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<UserDto>> GetUserById([FromRoute] int id) =>
        (await userService.GetUserById(id)).Match<ActionResult<UserDto>>(
             user => Ok(UserDto.FromUser(user)),
             notFound => NotFound());
    
    [HttpPost]
    [Route("")]
    [ProducesResponseType<UserDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<UserDto>> AddUser([FromBody] CreateUserRequest request)
    {
        if (!ValidateRequest<CreateUserRequest.Validator, CreateUserRequest>(request, out string[]? errors))
        {
            logger.LogInformation("Invalid add user request");
            return BadRequest(errors);
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success<User>, IUserService.Conflict> result = await userService.AddUser(request.Username);
            return await result.Match<ValueTask<ActionResult<UserDto>>>(
                async success =>
                {
                    await transaction.CommitAsync();
                    return CreatedAtAction(nameof(GetUserById), 
                                           new
                                           {
                                               id = success.Value.Id
                                           }, UserDto.FromUser(success.Value));
                },
                async conflict =>
                {
                    await transaction.RollbackAsync();
                    return Conflict("User already exists");
                });
            
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "Error while adding user");
            return Problem();
        }
    }
    
    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> DeleteUser([FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogInformation("Invalid user id: {Id}", id);
            return BadRequest("Invalid user id");
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success, NotFound> result = await userService.DeleteUser(id);
            return await result.Match<ValueTask<IActionResult>>(
                async success =>
                {
                    await transaction.CommitAsync();
                    return NoContent();
                },
                async notFound =>
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                });
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "Error while deleting user with id {Id}", id);
            return Problem();
        }
    }
}

public sealed record UserDto(int Id, string Username, Role Role)
{
    public static UserDto FromUser(User user) =>
        new(user.Id, user.Username, user.Role);
}

public sealed record CreateUserRequest(string Username)
{
    public sealed class Validator : AbstractValidator<CreateUserRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .MaximumLength(Const.MaxUsernameLength);
        }
    }
}