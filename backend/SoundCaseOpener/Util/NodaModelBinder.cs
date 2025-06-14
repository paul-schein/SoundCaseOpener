﻿using Microsoft.AspNetCore.Mvc.ModelBinding;
using NodaTime.Text;

namespace SoundCaseOpener.Util;

public sealed class NodaTimeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(Instant))
        {
            return new InstantModelBinder();
        }
        
        return context.Metadata.ModelType == typeof(LocalDate)
        ? new LocalDateModelBinder()
        : null;
    }
}

file abstract class NodaTimeModelBinder<T> : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }
        
        string? dateString = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(dateString))
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Empty date");
            
            return Task.CompletedTask;
        }
        
        ParseResult<T> parseResult = Parse(dateString);
        if (!parseResult.Success)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid date format");
            
            return Task.CompletedTask;
        }
        
        bindingContext.Result = ModelBindingResult.Success(parseResult.Value);
        
        return Task.CompletedTask;
    }
    
    protected abstract ParseResult<T> Parse(string dateString);
}

file sealed class LocalDateModelBinder : NodaTimeModelBinder<LocalDate>
{
    protected override ParseResult<LocalDate> Parse(string dateString) => LocalDatePattern.Iso.Parse(dateString);
}

file sealed class InstantModelBinder : NodaTimeModelBinder<Instant>
{
    protected override ParseResult<Instant> Parse(string dateString) => InstantPattern.ExtendedIso.Parse(dateString);
}
