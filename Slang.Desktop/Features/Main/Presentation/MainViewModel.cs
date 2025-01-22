using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Slang.Desktop.Features.Project.Presentation;

namespace Slang.Desktop.Features.Main.Presentation;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private ProjectViewModel _project = null!;

    [RelayCommand]
    private void OpenProject()
    {
        // todo: open with open file dialog
        Project = new ProjectViewModel();
    }
}