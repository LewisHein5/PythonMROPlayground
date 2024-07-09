namespace MRO.Models;

public record PythonClass
{
    public virtual bool Equals(PythonClass? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    private const string ObjectName = "Object";
    private readonly List<PythonClass> _parents = [];

    public PythonClass(string name)
    {
        Name = name;
    }

    public PythonClass(string name, List<PythonClass> parents)
    {
        Name = name;
        _parents = parents;
    }

    public string Name { get; } = "";

    public List<PythonClass> Parents
    {
        get
        {
            if (Name == ObjectName)
            {
                return [];
            }

            return _parents.Count == 0 ? [new PythonClass(ObjectName)] : _parents;
        }
    }
}