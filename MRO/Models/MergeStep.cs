namespace MRO.Models;

public record MergeStep
{
    public List<List<PythonClass>> UnmergedClasses = [];
    public List<PythonClass> MergedClasses = [];
}