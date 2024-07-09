namespace MRO.Models;

public record LinearizationStep
{
    public List<List<PythonClass>> ClassesList;
    public List<MergeStep> MergeSteps = [];
    public PythonClass Class;
}