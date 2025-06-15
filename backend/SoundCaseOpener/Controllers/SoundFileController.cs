using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Core.Services;
using SoundCaseOpener.Core.Util;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;
using SoundCaseOpener.Util;
using NotFound = OneOf.Types.NotFound;

namespace SoundCaseOpener.Controllers;

[Route("api/soundFiles")]
public class SoundFileController(ISoundFileService soundFileService,
                                 ITransactionProvider transaction,
                                 ILogger<SoundFileController> logger,
                                 IOptions<Settings> settings) : BaseController
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType<AllSoundFilesResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<AllSoundFilesResponse>> GetAllSoundFilesAsync() => 
        Ok(new AllSoundFilesResponse(
                                     (await soundFileService.GetAllSoundFilesAsync())
                                     .Select(SoundFileDto.FromSoundFile
                                             ).ToList()));

    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType<SoundFileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<SoundFileDto>> GetSoundFileById([FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogInformation("Invalid sound file id: {Id}", id);
            return BadRequest("Invalid sound file id");
        }
        
        return (await soundFileService.GetSoundFileByIdAsync(id))
            .Match<ActionResult<SoundFileDto>>(soundFile => Ok(SoundFileDto.FromSoundFile(soundFile)),
                                               notFound => NotFound());
    }
        

    [HttpPost]
    [Route("{name}")]
    [ProducesResponseType<SoundFileDto>(StatusCodes.Status201Created)]
    public async ValueTask<ActionResult<SoundFileDto>> AddSoundFileAsync([FromRoute] string name, 
                                                                         IFormFile file,
                                                                         CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Const.MaxSoundNameLength)
        {
            logger.LogInformation("Invalid sound file name: {Name}", name);
            return BadRequest("Invalid sound file name");
        }
        
        if (file.Length == 0)
        {
            logger.LogInformation("Empty file upload attempt");
            return BadRequest("File cannot be empty");
        }

        string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!settings.Value.AllowedFileExtensions.Contains(fileExtension)
            || !settings.Value.AllowedFileTypes.Contains(file.ContentType))
        {
            logger.LogInformation("Invalid file upload attempt: {FileName}", file.FileName);
            return BadRequest("Invalid file type");
        }

        try
        {
            await transaction.BeginTransactionAsync();

            SoundFile soundFile = await soundFileService.AddSoundFileAsync(name, 
                                                                           file.CopyToAsync, 
                                                                           fileExtension,
                                                                           cancellationToken);
            
            await transaction.CommitAsync();
            
            return CreatedAtAction(nameof(GetSoundFileById), 
                                            new
                                            {
                                                id = soundFile.Id
                                            },
                                            SoundFileDto.FromSoundFile(soundFile));
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "Error while adding sound file");
            return Problem();
        }
    }
    
    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> DeleteSoundFileAsync([FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogInformation("Invalid sound file id: {Id}", id);
            return BadRequest("Invalid sound file id");
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success, NotFound> result = await soundFileService.DeleteSoundFileAsync(id);
            return await result.Match<ValueTask<IActionResult>>(async success =>
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
            logger.LogError(e, "Error while deleting sound file");
            return Problem();
        }
    }
    
    [HttpPatch]
    [Route("{id:int}/name/{newName}")]
    [ProducesResponseType<SoundFileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> ChangeSoundFileNameAsync([FromRoute] int id, 
                                                                   [FromRoute] string newName)
    {
        if (id <= 0)
        {
            logger.LogInformation("Invalid sound file id: {Id}", id);
            return BadRequest("Invalid sound file id");
        }

        if (string.IsNullOrWhiteSpace(newName) || newName.Length > Const.MaxSoundNameLength)
        {
            logger.LogInformation("Invalid sound file name: {NewName}", newName);
            return BadRequest("Invalid sound file name");
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success<SoundFile>, NotFound> result = await soundFileService.ChangeSoundFileNameAsync(id, newName);
            return await result.Match<ValueTask<IActionResult>>(async success => 
                                                                { 
                                                                    await transaction.CommitAsync(); 
                                                                    return Ok(SoundFileDto.FromSoundFile(success.Value)); 
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
            logger.LogError(e, "Error while changing sound file name");
            return Problem();
        }
    }
}

public sealed record AllSoundFilesResponse(IReadOnlyCollection<SoundFileDto> SoundFiles);

public sealed record SoundFileDto(int Id, string Name, string FilePath)
{
    public static SoundFileDto FromSoundFile(SoundFile soundFile) =>
        new(soundFile.Id, soundFile.Name, soundFile.FilePath);
};
