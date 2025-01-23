using System.Globalization;

namespace Slang.Desktop.Features.Project.Domain;

public record SlangConfig(
    CultureInfo BaseCulture
);