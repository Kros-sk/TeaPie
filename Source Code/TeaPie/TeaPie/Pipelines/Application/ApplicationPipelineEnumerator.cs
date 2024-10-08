using System.Collections;
using TeaPie.Pipelines.Base;

namespace TeaPie.Pipelines.Application;
internal class ApplicationPipelineEnumerator : IEnumerator<IPipelineStep>
{
    private readonly List<IPipelineStep> _steps;
    private int _currentIndex;

    internal ApplicationPipelineEnumerator(List<IPipelineStep> steps)
    {
        _steps = steps;
        _currentIndex = -1;
    }

    public IPipelineStep Current => _steps[_currentIndex];

    object IEnumerator.Current => _steps[_currentIndex];

    public void Reset() => _currentIndex = -1;
    public bool MoveNext()
    {
        _currentIndex++;
        return _currentIndex < _steps.Count;
    }
    public bool MovePrevious()
    {
        _currentIndex--;
        return _currentIndex < _steps.Count && _currentIndex > 0;
    }

    public void Dispose() { }
}
