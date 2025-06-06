﻿using System.Collections;

namespace TeaPie.Pipelines;

internal class StepsCollection : IEnumerable<IPipelineStep>
{
    private readonly LinkedList<IPipelineStep> _steps = [];
    private readonly Dictionary<IPipelineStep, LinkedListNode<IPipelineStep>> _index = [];

    public int Count => _steps.Count;

    public void Insert(IPipelineStep predecessor, IPipelineStep step)
    {
        ArgumentNullException.ThrowIfNull(nameof(predecessor));
        ArgumentNullException.ThrowIfNull(nameof(step));

        if (_index.TryGetValue(predecessor, out var referenceNode))
        {
            var newNode = _steps.AddAfter(referenceNode, step);
            _index.Add(step, newNode);
        }
        else
        {
            throw new InvalidOperationException("Predecessor node could not be found.");
        }
    }

    public void Add(IPipelineStep step)
    {
        ArgumentNullException.ThrowIfNull(nameof(step));

        var node = _steps.AddLast(step);
        _index.Add(step, node);
    }

    public void InsertRange(IPipelineStep predecessor, IEnumerable<IPipelineStep> steps)
    {
        ArgumentNullException.ThrowIfNull(nameof(predecessor));
        ArgumentNullException.ThrowIfNull(nameof(steps));

        if (_index.TryGetValue(predecessor, out var referenceNode))
        {
            foreach (var step in steps)
            {
                var newNode = _steps.AddAfter(referenceNode, step);
                _index.Add(step, newNode);
                referenceNode = newNode;
            }
        }
        else
        {
            throw new InvalidOperationException("Predecessor node could not be found.");
        }
    }

    public void AddRange(IEnumerable<IPipelineStep> steps)
    {
        ArgumentNullException.ThrowIfNull(nameof(steps));

        foreach (var step in steps)
        {
            Add(step);
        }
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
