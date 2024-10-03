using System.Globalization;
using System.Xml.Linq;
using Project2015To2017.Definition;
using Slang.Gpt.Data;
using Slang.Gpt.Domain;
using Slang.Gpt.Domain.Utils;
using Slang.Shared;

namespace Slang.Gpt.Cli;

internal static class AdditionalFilesRepository
{
    public static SlangFileCollection GetFileCollection(
        Project project,
        string csProjDirectory,
        CultureInfo baseCulture)
    {
        var additionalFiles = project.ItemGroups.SelectMany(g => g.Elements())
            .Where(item => item.Name == "AdditionalFiles")
            .ToList();

        var files = GetFiles(additionalFiles, csProjDirectory);

        return FilesRepository.GetFileCollection(
            baseCulture: baseCulture,
            allFiles: files);
    }


    private static IEnumerable<FileInfo> GetFiles(List<XElement> additionalFiles, string csProjDirectory)
    {
        foreach (var additionalFile in additionalFiles)
        {
            if (additionalFile.HasAttributes)
            {
                var include = additionalFile.Attribute("Include");

                if (include != null)
                {
                    string patternOrFile = include.Value;

                    if (patternOrFile.Contains('*'))
                    {
                        string[] matchedFiles = Directory.GetFiles(csProjDirectory,
                            patternOrFile.Replace("\\", "/"), SearchOption.AllDirectories);

                        foreach (string file in matchedFiles)
                        {
                            var fileInfo = new FileInfo(file);

                            if (fileInfo.Exists && fileInfo.Name.EndsWith(Constants.AdditionalFilePattern))
                                yield return fileInfo;
                        }
                    }
                    else
                    {
                        string filePath = Path.Combine(csProjDirectory, patternOrFile);

                        var fileInfo = new FileInfo(filePath);

                        if (fileInfo.Exists && fileInfo.Name.EndsWith(Constants.AdditionalFilePattern))
                            yield return fileInfo;
                    }
                }
            }
        }
    }
}