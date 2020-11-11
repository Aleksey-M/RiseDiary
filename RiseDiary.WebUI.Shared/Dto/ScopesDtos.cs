using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RiseDiary.Shared.Dto
{
    public sealed class NewScopeDto
    {
        [Required, StringLength(50)]
        public string NewScopeName { get; set; } = null!;
    }

    public sealed class ThemeDto
    {
        public Guid ThemeId { get; set; }
        public string ThemeName { get; set; } = null!;
        public bool Actual { get; set; }
    }

    public sealed class ScopeDto
    {
        [Required]
        public Guid ScopeId { get; set; }
        [Required, StringLength(50)]
        public string ScopeName { get; set; } = null!;
        public IEnumerable<ThemeDto> Themes { get; set; } = null!;
    }

    public sealed class UpdateScopeDto
    {
        [Required]
        public Guid ScopeId { get; set; }
        [Required, StringLength(50)]
        public string NewScopeName { get; set; } = null!;
    }

    public sealed class NewThemeDto
    {
        [Required]
        public Guid ScopeId { get; set; }
        [Required, StringLength(50)]
        public string NewThemeName { get; set; } = null!;
        [Required]
        public bool Actual { get; set; }
    }

    public sealed class UpdateThemeDto
    {
        [Required]
        public Guid ScopeId { get; set; }
        [Required]
        public Guid ThemeId { get; set; }
        [Required, StringLength(50)]
        public string ThemeNewName { get; set; } = null!;
        [Required]
        public bool? NewActual { get; set; }
    }
}
