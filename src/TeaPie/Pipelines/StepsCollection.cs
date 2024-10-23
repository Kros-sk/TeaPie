using System.Collections;

namespace TeaPie.Pipelines;

internal class StepsCollection : IEnumerable<IPipelineStep>
{
    private readonly LinkedList<IPipelineStep> _steps = [];
    private readonly Dictionary<IPipelineStep, LinkedListNode<IPipelineStep>> _index = [];

    /// <summary>
    /// Insert <param cref="step"> just right after <param cref="predecessor">. If no predecessor is passed, step is
    /// added at the end of the colllection.
    /// </summary>
    /// <param name="step">Pipeline step to be added.</param>
    /// <param name="predecessor">Predecessor of the step. If null, the last element is considered as predecessor.</param>
    /// <returns>Returns whether step was successfully inserted.</returns>
    public bool Insert(IPipelineStep step, IPipelineStep? predecessor = null)
    {
        if (predecessor is null)
        {
            var node = _steps.AddLast(step);
            _index.Add(step, node);
            return true;
        }
        else if (_index.TryGetValue(predecessor, out var referenceNode))
        {
            var newNode = _steps.AddAfter(referenceNode, step);
            _index.Add(step, newNode);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Insert collection of <param cref="steps"> just right after <param cref="predecessor">. If no predecessor is passed,
    /// steps are added at the end of the colllection.
    /// </summary>
    /// <param name="steps">Collection of pipeline steps to be added.</param>
    /// <param name="predecessor">Predecessor of the steps. If null, the last element is considered as predecessor.</param>
    /// <returns>Returns whether steps were successfully inserted.</returns>
    public bool InsertRange(IEnumerable<IPipelineStep> steps, IPipelineStep? predecessor = null)
    {
        if (predecessor is null)
        {
            foreach (var step in steps)
            {
                Insert(step);
            }

            return true;
        }
        else if (_index.TryGetValue(predecessor, out var referenceNode))
        {
            foreach (var step in steps)
            {
                var newNode = _steps.AddAfter(referenceNode, step);
                _index.Add(step, newNode);
                referenceNode = newNode;
            }

            return true;
        }

        return false;
    }

    private LinkedListNode<IPipelineStep>? First() => _steps.First;

    public IEnumerator<IPipelineStep> GetEnumerator() => new StepsCollectionEnumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => new StepsCollectionEnumerator(this);

    /// <summary>
    /// Steps collection modification-resilient enumerator.
    /// In order to retrieve 'Current', call of 'MoveNext()' has to be done first.
    /// </summary>
    /// <param name="steps">Collection of the steps, which should be enumerated.</param>
    private class StepsCollectionEnumerator(StepsCollection steps) : IEnumerator<IPipelineStep>
    {
        private readonly StepsCollection _steps = steps;
        private LinkedListNode<IPipelineStep>? _currentNode;
        private bool _started;

        public IPipelineStep Current => GetCurrent();

        object IEnumerator.Current => GetCurrent();

        private IPipelineStep GetCurrent() => _currentNode is null
            ? throw new InvalidOperationException($"It is forbidden to access '{nameof(Current)}' before calling '" +
                $"{nameof(MoveNext)}()'")
            : _currentNode.Value;

        public bool MoveNext()
        {
            if (_currentNode is not null)
            {
                _currentNode = _currentNode.Next;
                return _currentNode is not null;
            }
            else if (!_started)
            {
                _started = true;
                _currentNode = _steps.First();
                return _currentNode is not null;
            }

            return false;
        }

        public void Reset()
        {
            _currentNode = null;
            _started = false;
        }

        public void Dispose() { }
    }
}
