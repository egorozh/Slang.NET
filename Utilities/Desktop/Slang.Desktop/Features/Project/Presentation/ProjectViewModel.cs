using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using Slang.Generator.Core.Nodes.Nodes;
using Slang.Generator.Core.NodesData;

namespace Slang.Desktop.Features.Project.Presentation;

public class ProjectViewModel : ObservableObject
{
    private readonly ObservableCollection<NodeViewModel> _nodes;

    public HierarchicalTreeDataGridSource<NodeViewModel> Source { get; }

    public ProjectViewModel(Domain.ProjectModel project)
    {
        var cultures = project.Nodes.OrderBy(x => x.BaseLocale).Select(x => x.Locale).ToArray();

        var baseData = project.Nodes.First(x => x.BaseLocale);

        _nodes = CreateNodes(baseData.Root, cultures, project.Nodes);

        Source = new HierarchicalTreeDataGridSource<NodeViewModel>(_nodes);
        Source.Columns.Add(
            new HierarchicalExpanderColumn<NodeViewModel>(
                new TextColumn<NodeViewModel, string>("Key", x => x.Key),
                x => x.Children)
        );
        Source.Columns.AddRange(cultures.Select(x =>
                new TextColumn<NodeViewModel, string>(
                    header: x.TwoLetterISOLanguageName,
                    getter: n => n.Cultures.First(y => y.Culture == x).Value)
            )
        );
        Source.ExpandAll();
    }

    private ObservableCollection<NodeViewModel> CreateNodes(ObjectNode root, CultureInfo[] cultures,
        List<I18NData> projectNodes)
    {
        var nodes = new ObservableCollection<NodeViewModel>();

        foreach ((string? key, var node) in root.Entries)
        {
            var nodeVm = new NodeViewModel
            {
                Key = key
            };

            if (node is StringTextNode)
            {
                nodeVm.Cultures = cultures.Select(culture =>
                {
                    var cultureData = projectNodes.First(x => Equals(x.Locale, culture));

                    if (cultureData.Root.Entries.TryGetValue(key, out var value) && value is StringTextNode textNode)
                    {
                        return new CultureValue
                        {
                            Culture = culture,
                            Value = textNode.Content
                        };
                    }

                    return new CultureValue
                    {
                        Culture = culture,
                        Value = "-"
                    };
                }).ToArray();
            }
            else if (node is ObjectNode objectNode)
            {
                nodeVm.Children = CreateNodes(objectNode, cultures,
                    projectNodes.Select(x =>
                    {
                        if (x.Root.Entries.TryGetValue(key, out var value))
                        {
                            if (value is ObjectNode n)
                            {
                                return x with { Root = n };
                            }
                          
                        }
                        
                        return new I18NData(x.BaseLocale, x.Locale, null);
                    }).ToList());
            }

            nodes.Add(nodeVm);
        }
        
        return nodes;
    }
}