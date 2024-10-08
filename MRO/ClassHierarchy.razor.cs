using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MRO.Models;
using QuikGraph;
using QuikGraph.Graphviz;

namespace MRO;

public partial class ClassHierarchy : ComponentBase
{
    private AdjacencyGraph<PythonClass, TaggedEdge<PythonClass, int>>? _graph = null;
    private List<PythonClass> _linearization = [];
    private List<LinearizationStep> _linearizationSteps = [];
    private async void UpdateGraph(IEnumerable<PythonClass> classes)
    {
        _graph = new AdjacencyGraph<PythonClass, TaggedEdge<PythonClass, int>>();
        var classesList = classes.ToList();
        foreach (var pythonClass in classesList)
        {
            AddClassToGraph(pythonClass);
        }
        
        var algorithm = new GraphvizAlgorithm<PythonClass, TaggedEdge<PythonClass, int>>(_graph);
        algorithm.FormatVertex += (_, args) =>
        {
            args.VertexFormat.Label = args.Vertex.Name;
        };
        /*algorithm.FormatEdge += (_, args) =>
        {
            var label = new GraphvizEdgeLabel
            {
                Value = (args.Edge.Tag + 1).ToString()
            };
            args.EdgeFormat.Label = label;
        };*/

        var graphString = algorithm.Generate().Replace("digraph G {", "digraph G { rankdir = \"BT\"");

        await JsRuntime.InvokeVoidAsync("renderDot", graphString, _inheritanceGraph);
        _linearizationSteps = [];
        if (classesList.Count > 0)
        {
            _linearization = Linearize(classesList.Last());
        }
        StateHasChanged();
    }

    private void AddClassToGraph(PythonClass pythonClass)
    {
        if (_graph == null)
            return;
        _graph.AddVertex(pythonClass);
        var i = 0;
        foreach (var parent in pythonClass.Parents)
        {
            if (!_graph.ContainsVertex(parent))
            {
                AddClassToGraph(parent);
            }
            _graph.AddEdge(new TaggedEdge<PythonClass, int>(pythonClass, parent, i));
            i++;
        }
    }
    
    private List<PythonClass> Linearize(PythonClass @class)
    {
        if (_graph == null)
            return [];
        if (!_graph.ContainsVertex(@class))
            return [];

        var superClasses = _graph.OutEdges(@class)
            .OrderBy(e => e.Tag)
            .Select(e => e.Target)
            .ToList();
        if (superClasses.Count == 0)
            return [@class];

        
        //TODO: Clean this up
        try
        {
            var linearizedSuperClasses = superClasses.Select(Linearize).ToList();
            var currentLinearizationStep = new LinearizationStep { Class = @class, ClassesList = linearizedSuperClasses };
            _linearizationSteps.Add(currentLinearizationStep);
            linearizedSuperClasses.Add(superClasses); //preserves order in C3 linearization

            var merged = Merge(linearizedSuperClasses, ref currentLinearizationStep);
            if (merged == null)
            {
                throw new Exception("Cannot merge");
            }

            return merged.Prepend(@class).ToList();
        }
        catch
        {
            return [];
        }
    }

    private static List<PythonClass>? Merge(List<List<PythonClass>> classes, ref LinearizationStep currentStep)
    {
        var merged = new List<PythonClass>();
        PythonClass? candidate;
        while (classes.Sum(x=>x.Count) > 0)
        {
            candidate = null;
            foreach (var classList in classes)
            {
                var heads = classes.Select(list => list.First()).ToList();
                var tails = classes.SelectMany(list => list.Skip(1)).ToList();
                candidate = classList.FirstOrDefault(@class => heads.Contains(@class) && !tails.Contains(@class));
                if (candidate != null)
                {
                    break;
                }
            }

            if (candidate == null)
                return null; //cannot linearize

            classes = classes.Select(list => list.Where(@class => @class.Name != candidate.Name).ToList())
                .Where(list => list.Count > 0)
                .ToList();
            merged.Add(candidate);
            currentStep.MergeSteps.Add(new MergeStep {UnmergedClasses = classes, MergedClasses = merged.AsEnumerable().ToList()});
        }

        return merged;
    }
}