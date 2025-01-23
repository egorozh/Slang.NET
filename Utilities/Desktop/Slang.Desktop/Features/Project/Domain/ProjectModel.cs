using Slang.Generator.Core.NodesData;

namespace Slang.Desktop.Features.Project.Domain;

public record ProjectModel(string CsProjectPath, List<I18NData> Nodes);