using MRO.Models;

namespace MRO.Services;

public delegate void CodeChangedEventHandler(IEnumerable<PythonClass> classes);

public class UpdateService
{
    public event CodeChangedEventHandler CodeChangedEvent;
    public void UpdateClassList(IEnumerable<PythonClass> classes)
    {
        CodeChangedEvent?.Invoke(classes);
    }
}