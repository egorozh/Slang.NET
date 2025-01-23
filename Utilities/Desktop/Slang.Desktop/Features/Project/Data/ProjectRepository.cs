using Slang.Desktop.Features.Project.Domain;

namespace Slang.Desktop.Features.Project.Data;

public class ProjectRepository
{
    public async Task<ProjectModel> OpenProject(string filePath)
    {
        //todo: get slang config, locales and e.g.

        return new ProjectModel(filePath);
    }
}