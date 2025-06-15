using OneOf;
using OneOf.Types;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SoundCaseOpener.Core.Services;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;
using SoundCaseOpener.Util;

namespace SoundCaseOpener.Controllers;

[Route("api/sound-templates")]
public class SoundTemplateController(ISoundTemplateService soundTemplateService, 
                                     ITransactionProvider transaction, 
                                     ILogger<SoundTemplateController> logger) : BaseController
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType<AllSoundTemplatesResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<AllSoundTemplatesResponse>> GetAllSoundTemplatesAsync() =>
        Ok(new AllSoundTemplatesResponse((await soundTemplateService.GetAllAsync())
                                         .Select(SoundTemplateDto.FromSoundTemplate).ToList()));
    
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType<SoundTemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<SoundTemplateDto>> GetSoundTemplateById([FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogInformation("Invalid sound template id: {Id}", id);
            return BadRequest("Invalid sound template id");
        }

        return (await soundTemplateService.GetByIdAsync(id))
            .Match<ActionResult<SoundTemplateDto>>(template => Ok(SoundTemplateDto.FromSoundTemplate(template)),
                                                   notFound => NotFound());
    }
    
    [HttpPost]
    [Route("")]
    [ProducesResponseType<SoundTemplateDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<SoundTemplateDto>> AddSoundTemplateAsync(
        [FromBody] CreateSoundTemplateRequest request)
    {
        if (!ValidateRequest<CreateSoundTemplateRequest.Validator, CreateSoundTemplateRequest>(
             request, out string[]? errors))
        {
            logger.LogInformation("Invalid sound template request");
            return BadRequest(errors);
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success<SoundTemplate>, NotFound> result = 
                await soundTemplateService.AddAsync(request.Name,
                                                    request.Description,
                                                    request.Rarity,
                                                    request.MinCooldown,
                                                    request.MaxCooldown,
                                                    request.SoundFileId);
            
            return await result.Match<ValueTask<ActionResult<SoundTemplateDto>>>(
                async success =>
                {
                    await transaction.CommitAsync();
                    return CreatedAtAction(nameof(GetSoundTemplateById), 
                                           new
                                           {
                                               id = success.Value.Id
                                           }, 
                                           SoundTemplateDto.FromSoundTemplate(success.Value));
                },
                async notFound =>
                {
                    await transaction.RollbackAsync();
                    logger.LogWarning("Sound file with id {Id} not found", request.SoundFileId);
                    return NotFound();
                });
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "Error while adding sound template");
            return Problem();
        }
    }
    
    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> DeleteSoundTemplateAsync([FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogInformation("Invalid sound template id: {Id}", id);
            return BadRequest("Invalid sound template id");
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success, NotFound> result = await soundTemplateService.DeleteAsync(id);
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
            logger.LogError(e, "Error while deleting sound template");
            return Problem();
        }
    }
}

public sealed record CreateSoundTemplateRequest(
    string Name,
    string Description,
    Rarity Rarity,
    int MinCooldown,
    int MaxCooldown,
    int SoundFileId)
{
    public sealed class Validator : AbstractValidator<CreateSoundTemplateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(Const.MaxItemNameLength);
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(Const.MaxItemDescriptionLength);
            RuleFor(x => x.Rarity).IsInEnum();
            RuleFor(x => x.MinCooldown).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MaxCooldown)
                .GreaterThanOrEqualTo(x => x.MinCooldown);
            RuleFor(x => x.SoundFileId).GreaterThan(0);
        }
    }
}

public sealed record AllSoundTemplatesResponse(IReadOnlyCollection<SoundTemplateDto> SoundTemplates);

public sealed record SoundTemplateDto(
    int Id,
    string Name,
    string Description,
    Rarity Rarity,
    int MinCooldown,
    int MaxCooldown,
    int SoundFileId)
{
    public static SoundTemplateDto FromSoundTemplate(SoundTemplate template) => new(
     template.Id,
     template.Name,
     template.Description,
     template.Rarity,
     template.MinCooldown,
     template.MaxCooldown,
     template.SoundFileId);
}