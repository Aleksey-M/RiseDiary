using System.ComponentModel.DataAnnotations;

namespace RiseDiary.Shared.Images;

public sealed class UpdateImageNameDto
{
    [Required]
    public Guid ImageId { get; set; }

    [Required, StringLength(250)]
    public string ImageNewName { get; set; } = "";
}
