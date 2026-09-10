using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RiseDiary.Common.Core;

namespace RiseDiary.Common.Settings.Validators;

public sealed class SettingsDtoValidator : ISettingsDtoValidator
{
    private readonly IServiceProvider _provider;

    public SettingsDtoValidator(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Result ValidateDto(object? dtoObject)
    {
        if (dtoObject == null || dtoObject is not ISettingsDto)
        {
            return Result.Failure("Неправильный тип объекта настройки");
        }

        return dtoObject switch
        {
            ImagesSettingsDto imagesSettingsDto => ValidateDto(imagesSettingsDto),
            PagesSizesSettingsDto pagesSizesSettingsDto => ValidateDto(pagesSizesSettingsDto),
            ImportantDaysSettingsDto importantDaysSettingsDto => ValidateDto(importantDaysSettingsDto),
            _ => Result.Success()
        };
    }

    public Result ValidateDto<T>(T dto) where T : ISettingsDto
    {
        var validator = _provider.GetService<AbstractValidator<T>>();
        if (validator == null)
        {
            return Result.Success();
        }

        var validationResult = validator.Validate(dto);
        return validationResult.IsValid
            ? Result.Success()
            : Result.Failure(string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage)));
    }
}
