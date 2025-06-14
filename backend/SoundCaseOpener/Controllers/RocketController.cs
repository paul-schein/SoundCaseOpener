﻿using FluentValidation;
using SoundCaseOpener.Core.Logic;
using SoundCaseOpener.Core.Services;
using SoundCaseOpener.Persistence;
using SoundCaseOpener.Persistence.Model;
using SoundCaseOpener.Persistence.Util;
using SoundCaseOpener.Util;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;

namespace SoundCaseOpener.Controllers;

[Route("api/rockets")]
public sealed class RocketController(
    ITransactionProvider transaction,
    IRocketService rocketService,
    ILogger<RocketController> logger) : BaseController
{
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType<RocketDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<RocketDto>> GetRocketById([FromRoute] int id)
    {
        if (id < 0)
        {
            return BadRequest();
        }

        OneOf<Rocket, NotFound> rocketResult = await rocketService.GetRocketByIdAsync(id, false);

        return rocketResult.Match<ActionResult<RocketDto>>(rocket => Ok(RocketDto.FromRocket(rocket)),
                                                           notFound => NotFound());
    }

    [HttpGet]
    [Route("")]
    [ProducesResponseType<AllRocketsResponse>(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<AllRocketsResponse>> GetAllRockets()
    {
        IReadOnlyCollection<Rocket> rockets = await rocketService.GetAllRocketsAsync();

        return Ok(new AllRocketsResponse
        {
            Rockets = rockets.Select(RocketDto.FromRocket).ToList()
        });
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> AddRocket([FromBody] AddRocketRequest addRequest)
    {
        if (!ValidateRequest<AddRocketRequest.Validator, AddRocketRequest>(addRequest))
        {
            return BadRequest();
        }

        try
        {
            await transaction.BeginTransactionAsync();

            var rocket = await rocketService.AddRocketAsync(addRequest.Manufacturer, addRequest.ModelName,
                                                            addRequest.MaxThrust, addRequest.PayloadDeltaV);
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetRocketById), new
            {
                rocket.Id
            }, RocketDto.FromRocket(rocket));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add rocket");
            await transaction.RollbackAsync();

            return Problem();
        }
    }

    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> DeleteRocket([FromRoute] int id)
    {
        try
        {
            await transaction.BeginTransactionAsync();

            OneOf<Success, NotFound> deleteResult = await rocketService.DeleteRocketAsync(id);

            return await deleteResult.Match<ValueTask<IActionResult>>(async success =>
            {
                await transaction.CommitAsync();

                return NoContent();
            }, async notFound =>
            {
                await transaction.RollbackAsync();

                return NotFound();
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete rocket");
            await transaction.RollbackAsync();

            return Problem();
        }
    }
}

public sealed class AddRocketRequest
{
    public required string ModelName { get; set; }
    public required string Manufacturer { get; set; }
    public double MaxThrust { get; set; }
    public long PayloadDeltaV { get; set; }

    public sealed class Validator : AbstractValidator<AddRocketRequest>
    {
        public Validator()
        {
            RuleFor(x => x.ModelName).NotEmpty();
            RuleFor(x => x.Manufacturer).NotEmpty();
            RuleFor(x => x.MaxThrust).GreaterThan(0);
            RuleFor(x => x.PayloadDeltaV).GreaterThan(0);
        }
    }
}

public sealed class AllRocketsResponse
{
    public required List<RocketDto> Rockets { get; set; }
}

public sealed class RocketDto
{
    public int Id { get; set; }
    public required string ModelName { get; set; }
    public required string Manufacturer { get; set; }
    public double MaxThrust { get; set; }
    public long PayloadDeltaV { get; set; }
    
    public static RocketDto FromRocket(Rocket rocket) =>
        new()
        {
            Id = rocket.Id,
            ModelName = rocket.ModelName,
            Manufacturer = rocket.Manufacturer,
            MaxThrust = rocket.MaxThrust,
            PayloadDeltaV = rocket.PayloadDeltaV
        };
}