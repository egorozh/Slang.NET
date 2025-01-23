using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Slang.Desktop.Features.Main.Presentation;
using Slang.Desktop.Features.Project.Data;

namespace Slang.Desktop;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);


    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(new ProjectRepository())
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}