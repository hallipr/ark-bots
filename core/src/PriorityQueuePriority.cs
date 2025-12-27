using System;
using System.Collections.Generic;

namespace ArkBotFramework.Tasks
{
    /// <summary>
    /// Priority queue ordered by priority level (highest first), then execution time.
    /// Used for active tasks that should run immediately in priority order.
    /// </summary>
    public class PriorityQueuePriority
    {
        private readonly SortedSet<QueueEntry> _queue;
        private int _insertionOrder;

        public PriorityQueuePriority()
        {
            _queue = new SortedSet<QueueEntry>(new QueueEntryComparer());
            _insertionOrder = 0;
        }

        /// <summary>
        /// Add a task to the queue with priority and execution time.
        /// </summary>
        /// <param name="task">The task to add</param>
        /// <param name="priority">Task priority level (lower number = higher priority)</param>
        /// <param name="executionTime">Unix timestamp when task was added</param>
        public void Add(ITask task, int priority, double executionTime)
        {
            var entry = new QueueEntry(priority, executionTime, _insertionOrder++, task);
            _queue.Add(entry);
        }

        /// <summary>
        /// Remove and return the task with the highest priority.
        /// </summary>
        /// <returns>QueueEntry tuple or null if empty</returns>
        public QueueEntry? Pop()
        {
            if (IsEmpty())
                return null;

            var entry = _queue.Min;
            if (entry != null)
                _queue.Remove(entry);
            
            return entry;
        }

        /// <summary>
        /// View the task with the highest priority without removing it.
        /// </summary>
        /// <returns>QueueEntry tuple or null if empty</returns>
        public QueueEntry? Peek()
        {
            if (IsEmpty())
                return null;

            return _queue.Min;
        }

        /// <summary>
        /// Check if the queue is empty.
        /// </summary>
        /// <returns>True if empty, false otherwise</returns>
        public bool IsEmpty()
        {
            return _queue.Count == 0;
        }

        /// <summary>
        /// Get the current size of the queue.
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// Represents an entry in the priority queue.
        /// </summary>
        public class QueueEntry
        {
            public int Priority { get; }
            public double ExecutionTime { get; }
            public int InsertionOrder { get; }
            public ITask Task { get; }

            public QueueEntry(int priority, double executionTime, int insertionOrder, ITask task)
            {
                Priority = priority;
                ExecutionTime = executionTime;
                InsertionOrder = insertionOrder;
                Task = task;
            }

            public void Deconstruct(out int priority, out double executionTime, out int insertionOrder, out ITask task)
            {
                priority = Priority;
                executionTime = ExecutionTime;
                insertionOrder = InsertionOrder;
                task = Task;
            }
        }

        /// <summary>
        /// Comparer for queue entries that orders by priority, then execution time, then insertion order.
        /// </summary>
        private class QueueEntryComparer : IComparer<QueueEntry>
        {
            public int Compare(QueueEntry? x, QueueEntry? y)
            {
                if (x == null || y == null)
                    return x == y ? 0 : (x == null ? -1 : 1);

                // First compare by priority (lower number = higher priority)
                int priorityComparison = x.Priority.CompareTo(y.Priority);
                if (priorityComparison != 0)
                    return priorityComparison;

                // If priorities are equal, compare by execution time
                int timeComparison = x.ExecutionTime.CompareTo(y.ExecutionTime);
                if (timeComparison != 0)
                    return timeComparison;

                // If execution times are also equal, compare by insertion order (FIFO for ties)
                return x.InsertionOrder.CompareTo(y.InsertionOrder);
            }
        }
    }
}
