using RiseDiary.Common.Core;

namespace RiseDiary.Common.Settings.Validators;

public interface ISettingsDtoValidator
{
    Result ValidateDto(object? dtoObject);
    Result ValidateDto<T>(T dto) where T : ISettingsDto;
}
