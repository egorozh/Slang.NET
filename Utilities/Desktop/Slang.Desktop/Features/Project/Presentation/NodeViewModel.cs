using System.Collections.ObjectModel;

namespace Slang.Desktop.Features.Project.Presentation;

public class NodeViewModel
{
    public string? Key { get; set; }

    public CultureValue[] Cultures { get; set; }

    public ObservableCollection<NodeViewModel> Children { get; set; } = new();
}