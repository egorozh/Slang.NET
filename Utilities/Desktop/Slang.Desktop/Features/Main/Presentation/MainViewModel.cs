using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Slang.Desktop.Features.Project.Data;
using Slang.Desktop.Features.Project.Presentation;

namespace Slang.Desktop.Features.Main.Presentation;

public partial class MainViewModel(ProjectRepository repository) : ObservableObject
{
    [ObservableProperty] private ProjectViewModel _project = null!;

    [ObservableProperty] private bool _isLoading;

    [RelayCommand]
    private async Task OpenProject(Window window)
    {
        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("C# project file")
                {
                    Patterns = ["*.csproj"]
                }
            ]
        };

        IsLoading = true;
        
        var result = await window.StorageProvider.OpenFilePickerAsync(options);

        if (!result.Any())
            return;
        
        var project = await repository.OpenProject(result[0].Path.AbsolutePath);
        
        IsLoading = false;
        
        Project = new ProjectViewModel(project);
    }
}