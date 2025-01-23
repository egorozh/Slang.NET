using Project2015To2017.Reading;
using Slang.Desktop.Features.Project.Domain;
using Slang.Generator.Core.Data;
using Slang.Generator.Core.NodesData;
using Slang.Utilities.Core.Translate;
using SlangFileCollection = Slang.Generator.Core.Data.SlangFileCollection;
using TranslationFile = Slang.Generator.Core.Data.TranslationFile;

namespace Slang.Desktop.Features.Project.Data;

public class ProjectRepository
{
    public async Task<ProjectModel> OpenProject(string filePath)
    {
        ProjectReader reader = new();
        var project = reader.Read(filePath);

        var fileInfo = new FileInfo(filePath);

        string csProjDirectoryPath = fileInfo.Directory!.FullName;
        var configResult = ConfigRepository.GetConfig(project, csProjDirectoryPath);

        if (configResult.TryPickT1(out var error, out var slangConfig))
        {
            //ShowGetConfigError(error);
            return null;
        }

        var fileCollection = AdditionalFilesRepository.GetFileCollection(
            project,
            csProjDirectoryPath,
            slangConfig.BaseCulture
        );

        var translationMap = await TranslationsRepository.Build(slangConfig.BaseCulture,
            fileCollection: new SlangFileCollection
            {
                Files = fileCollection.Files.Select(f => new TranslationFile(
                    Read: f.Read,
                    Locale: f.Locale
                )).ToList()
            });

        
        var translationModelList = NodesDataRepository.GetNodesData(
            slangConfig.BaseCulture,
            translationMap,
            Generator.Core.Entities.PluralAuto.Ordinal,
            "n"
        );
        
        return new ProjectModel(filePath);
    }
}