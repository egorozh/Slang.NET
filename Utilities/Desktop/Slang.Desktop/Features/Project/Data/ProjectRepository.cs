using Project2015To2017.Reading;
using Slang.Desktop.Features.Project.Domain;
using Slang.Utilities.Core.Translate;

namespace Slang.Desktop.Features.Project.Data;

public class ProjectRepository
{
    public async Task<ProjectModel> OpenProject(string filePath)
    {
        ProjectReader reader = new();
        var project = reader.Read(filePath);
        
        var fileInfo = new FileInfo(filePath);

        string csProjDirectoryPath = fileInfo.Directory!.FullName;
        var gptConfigResult = ConfigRepository.GetConfig(project, csProjDirectoryPath);

        if (gptConfigResult.TryPickT1(out var error, out var gptConfig))
        {
            //ShowGetConfigError(error);
            return null;
        }
        
        var fileCollection = AdditionalFilesRepository.GetFileCollection(
            project,
            csProjDirectoryPath,
            gptConfig.BaseCulture
        );
        
        return new ProjectModel(filePath);
    }
}