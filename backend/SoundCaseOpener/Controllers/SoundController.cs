using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Core.Services;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;
using SoundCaseOpener.Util;

namespace SoundCaseOpener.Controllers;

[Route("api/sounds")]
public class SoundController(ISoundService soundService,
                             ITransactionProvider transaction,
                             ILogger<SoundController> logger) : BaseController
{
    [HttpGet]
    [Route("{userId:int}")]
    [ProducesResponseType<AllSoundsOfUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<AllSoundsOfUserResponse>> GetAllSoundsOfUserAsync([FromRoute] int userId)
    {
        if (userId <= 0)
        {
            logger.LogInformation("Invalid user id: {UserId}", userId);
            return BadRequest();
        }

        OneOf<Success<IReadOnlyCollection<Sound>>, NotFound> result =
            await soundService.GetAllSoundsOfUserAsync(userId);
        
        return result.Match<ActionResult<AllSoundsOfUserResponse>>(success =>
            Ok(new AllSoundsOfUserResponse(success.Value.Select(SoundDto.FromSound).ToList())),
            notFound => NotFound());
    }
    
    [HttpPatch]
    [Route("{id:int}/name/{newName}")]
    [ProducesResponseType<SoundDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<SoundDto>> ChangeSoundNameAsync([FromRoute] int id, 
                                                                        [FromRoute] string newName)
    {
        if (id <= 0 || string.IsNullOrWhiteSpace(newName) || newName.Length > Const.MaxItemNameLength)
        {
            logger.LogInformation("Invalid sound id or name: {Id}, {NewName}", id, newName);
            return BadRequest();
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success<Sound>, NotFound> result = await soundService.ChangeSoundNameAsync(id, newName);
            return await result.Match<ValueTask<ActionResult<SoundDto>>>(
                async success =>
                {
                    await transaction.CommitAsync();
                    return Ok(SoundDto.FromSound(success.Value));
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
            logger.LogError(e, "Error while changing sound name for id {Id} to {NewName}", id, newName);
            return Problem();
        }
    }
}

public sealed record AllSoundsOfUserResponse(IReadOnlyCollection<SoundDto> Sounds);

public sealed record SoundDto(int Id, string Name, string Description, Rarity Rarity, int Cooldown, string FilePath)
{
    public static SoundDto FromSound(Sound sound) => 
        new(sound.Id, sound.Name, 
            sound.Template.Description, 
            sound.Template.Rarity,
            sound.Cooldown,
            ((SoundTemplate)sound.Template).SoundFile.FilePath);
}