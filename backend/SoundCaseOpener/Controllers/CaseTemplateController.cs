using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;
using SoundCaseOpener.Core.Services;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Repositories;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Shared;
using SoundCaseOpener.Util;

namespace SoundCaseOpener.Controllers;

[Route("api/case-templates")]
public class CaseTemplateController(ICaseTemplateService caseTemplateService,
                                    ITransactionProvider transaction,
                                    ILogger<CaseTemplateController> logger) : BaseController
{
    [HttpGet]
    [Route("")]
    [ProducesResponseType<AllCaseTemplatesResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<AllCaseTemplatesResponse>> GetAllCaseTemplatesAsync() =>
        Ok(new AllCaseTemplatesResponse(
            (await caseTemplateService.GetAllAsync())
            .Select(CaseTemplateDto.FromCaseTemplate).ToList()));

    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType<CaseTemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<CaseTemplateDto>> GetCaseTemplateById([FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogInformation("Invalid case template id: {Id}", id);
            return BadRequest("Invalid case template id");
        }

        return (await caseTemplateService.GetByIdAsync(id))
            .Match<ActionResult<CaseTemplateDto>>(caseTemplate => Ok(CaseTemplateDto.FromCaseTemplate(caseTemplate)),
                                                  notFound => NotFound());
    }
    
    [HttpPost]
    [Route("")]
    [ProducesResponseType<CaseTemplateDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<ActionResult<CaseTemplateDto>> CreateCaseTemplateAsync(
        [FromBody] CreateCaseTemplateRequest request)
    {
        if (!ValidateRequest<CreateCaseTemplateRequest.Validator, CreateCaseTemplateRequest>(
             request, out string[]? errors))
        {
            logger.LogInformation("Invalid create case template request");
            return BadRequest(errors);
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            CaseTemplate caseTemplate = await caseTemplateService.AddAsync(request.Name, 
                                                                           request.Description, 
                                                                           request.Rarity);
            
            await transaction.CommitAsync();
            
            return CreatedAtAction(nameof(GetCaseTemplateById), 
                                   new
                                   {
                                       id = caseTemplate.Id
                                   }, CaseTemplateDto.FromCaseTemplate(caseTemplate));
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "Error while creating case template");
            return Problem();
        }
    }
    
    [HttpPost]
    [Route("add-item-template")]
    [ProducesResponseType<CaseTemplateDto>(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> AddItemTemplateToCaseTemplateAsync(
        [FromBody] AddItemTemplateToCaseTemplateRequest request)
    {
        if (!ValidateRequest<AddItemTemplateToCaseTemplateRequest.Validator, AddItemTemplateToCaseTemplateRequest>(
             request, out string[]? errors))
        {
            logger.LogInformation("Invalid add item template to case template request");
            return BadRequest(errors);
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success, ICaseTemplateService.Conflict, NotFound> result =
                await caseTemplateService.AddItemTemplateToCaseTemplateAsync(request.CaseTemplateId,
                                                                             request.ItemTemplateId, 
                                                                             request.Weight);
            
            return await result.Match<ValueTask<IActionResult>>(
                async success =>
                {
                    await transaction.CommitAsync();
                    return NoContent();
                },
                async conflict =>
                {
                    await transaction.RollbackAsync();
                    return Conflict("Item template already exists in case template");
                },
                async notFound =>
                {
                    await transaction.RollbackAsync();
                    return NotFound("Case template or item template not found");
                });
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "Error while adding item template to case template");
            return Problem();
        }
    }
    
    [HttpDelete]
    [Route("{caseTemplateId:int}/item-template/{itemTemplateId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> RemoveItemTemplateFromCaseTemplateAsync(
        [FromRoute] int caseTemplateId, [FromRoute] int itemTemplateId)
    {
        if (caseTemplateId <= 0 || itemTemplateId <= 0)
        {
            logger.LogInformation("Invalid case template id: {CaseTemplateId} or item template id: {ItemTemplateId}",
                                  caseTemplateId, itemTemplateId);
            return BadRequest("Invalid case template id or item template id");
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success, NotFound> result =
                await caseTemplateService.RemoveItemTemplateFromCaseTemplateAsync(caseTemplateId, 
                                                                                  itemTemplateId);
            
            return await result.Match<ValueTask<IActionResult>>(
                async success =>
                {
                    await transaction.CommitAsync();
                    return NoContent();
                },
                async notFound =>
                {
                    await transaction.RollbackAsync();
                    return NotFound("Case template or item template not found");
                });
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError(e, "Error while removing item template from case template");
            return Problem();
        }
    }
    
    [HttpGet]
    [Route("{id:int}/sound-templates")]
    [ProducesResponseType<AllSoundTemplateCaseItemsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<AllSoundTemplateCaseItemsResponse>> GetSoundTemplatesInCaseTemplateByIdAsync(
        [FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogInformation("Invalid case template id: {Id}", id);
            return BadRequest("Invalid case template id");
        }

        OneOf<Success<IReadOnlyCollection<ICaseItemRepository.CaseItemSoundTemplate>>, NotFound> result = 
            await caseTemplateService.GetAllSoundTemplatesInCaseTemplateAsync(id);

        return result.Match<ActionResult<AllSoundTemplateCaseItemsResponse>>(
                                        success => Ok(new AllSoundTemplateCaseItemsResponse(
                                        success.Value.Select(SoundTemplateCaseItemDto.FromSoundTemplate).ToList())),
                                        notFound => NotFound("Case template not found"));;
    }
    
    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> DeleteCaseTemplateAsync([FromRoute] int id)
    {
        if (id <= 0)
        {
            logger.LogInformation("Invalid case template id: {Id}", id);
            return BadRequest("Invalid case template id");
        }

        try
        {
            await transaction.BeginTransactionAsync();
            
            OneOf<Success, NotFound> result = await caseTemplateService.DeleteAsync(id);
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
            logger.LogError(e, "Error while deleting case template");
            return Problem();
        }
    }
    
}

public sealed record AllSoundTemplateCaseItemsResponse(IReadOnlyCollection<SoundTemplateCaseItemDto> SoundTemplates);

public sealed record SoundTemplateCaseItemDto(int Id, 
                                              string Name, 
                                              string Description, 
                                              Rarity Rarity,
                                              int MinCooldown,
                                              int MaxCooldown,
                                              int SoundFileId,
                                              double Weight)
{
    public static SoundTemplateCaseItemDto FromSoundTemplate(ICaseItemRepository.CaseItemSoundTemplate soundTemplate) =>
        new(soundTemplate.Template.Id, 
            soundTemplate.Template.Name,
            soundTemplate.Template.Description,
            soundTemplate.Template.Rarity,
            soundTemplate.Template.MinCooldown,
            soundTemplate.Template.MaxCooldown,
            soundTemplate.Template.SoundFileId,
            soundTemplate.Weight);
}

public sealed record AddItemTemplateToCaseTemplateRequest(int CaseTemplateId, int ItemTemplateId, double Weight)
{
    public sealed class Validator : AbstractValidator<AddItemTemplateToCaseTemplateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CaseTemplateId)
                .GreaterThan(0);
            RuleFor(x => x.ItemTemplateId)
                .GreaterThan(0);
            RuleFor(x => x.Weight)
                .GreaterThan(0);
        }
    }
}

public sealed record CreateCaseTemplateRequest(string Name, string Description, Rarity Rarity)
{
    public sealed class Validator : AbstractValidator<CreateCaseTemplateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(Const.MaxItemNameLength);
            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(Const.MaxItemDescriptionLength);
            RuleFor(x => x.Rarity)
                .IsInEnum();
        }
    }
}

public sealed record AllCaseTemplatesResponse(IReadOnlyCollection<CaseTemplateDto> CaseTemplates);

public sealed record CaseTemplateDto(int Id, string Name, string Description, Rarity Rarity)
{
    public static CaseTemplateDto FromCaseTemplate(CaseTemplate caseTemplate) =>
        new(caseTemplate.Id, caseTemplate.Name, caseTemplate.Description, caseTemplate.Rarity);
};
