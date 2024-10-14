using System.Collections;

namespace TeaPie.Pipelines;

internal class StepsCollection
{
    private readonly LinkedList<IPipelineStep> _steps = [];
    private readonly Dictionary<IPipelineStep, LinkedListNode<IPipelineStep>> _index = [];

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

    private LinkedListNode<IPipelineStep>? First() => _steps.First;

    public IEnumerator<IPipelineStep?> GetEnumerator() => new StepsCollectionEnumerator(this);

    private class StepsCollectionEnumerator : IEnumerator<IPipelineStep?>
    {
        private readonly StepsCollection _steps;
        private LinkedListNode<IPipelineStep>? _currentNode;
        private bool _started;

        public StepsCollectionEnumerator(StepsCollection steps)
        {
            _steps = steps;
            _currentNode = null;
            _started = false;
        }

        public IPipelineStep? Current => _currentNode?.Value;

        object? IEnumerator.Current => _currentNode?.Value;

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

        public void Dispose() => throw new NotImplementedException();
    }
}
