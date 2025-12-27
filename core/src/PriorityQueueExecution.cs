using System;
using System.Collections.Generic;

namespace ArkBotFramework.Tasks
{
    /// <summary>
    /// Priority queue ordered by execution time (earliest first).
    /// Used for scheduling tasks based on when they should run.
    /// </summary>
    public class PriorityQueueExecution
    {
        private readonly SortedSet<QueueEntry> _queue;
        private int _insertionOrder;

        public PriorityQueueExecution()
        {
            _queue = new SortedSet<QueueEntry>(new QueueEntryComparer());
            _insertionOrder = 0;
        }

        /// <summary>
        /// Add a task to the queue with priority and execution time.
        /// </summary>
        /// <param name="task">The task to add</param>
        /// <param name="priority">Task priority level</param>
        /// <param name="executionTime">Unix timestamp when task should execute</param>
        public void Add(ITask task, int priority, double executionTime)
        {
            var entry = new QueueEntry(executionTime, _insertionOrder++, priority, task);
            _queue.Add(entry);
        }

        /// <summary>
        /// Remove and return the task with the earliest execution time.
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
        /// View the task with the earliest execution time without removing it.
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
            public double ExecutionTime { get; }
            public int InsertionOrder { get; }
            public int Priority { get; }
            public ITask Task { get; }

            public QueueEntry(double executionTime, int insertionOrder, int priority, ITask task)
            {
                ExecutionTime = executionTime;
                InsertionOrder = insertionOrder;
                Priority = priority;
                Task = task;
            }

            public void Deconstruct(out double executionTime, out int insertionOrder, out int priority, out ITask task)
            {
                executionTime = ExecutionTime;
                insertionOrder = InsertionOrder;
                priority = Priority;
                task = Task;
            }
        }

        /// <summary>
        /// Comparer for queue entries that orders by execution time, then insertion order.
        /// </summary>
        private class QueueEntryComparer : IComparer<QueueEntry>
        {
            public int Compare(QueueEntry? x, QueueEntry? y)
            {
                if (x == null || y == null)
                    return x == y ? 0 : (x == null ? -1 : 1);

                // First compare by execution time
                int timeComparison = x.ExecutionTime.CompareTo(y.ExecutionTime);
                if (timeComparison != 0)
                    return timeComparison;

                // If execution times are equal, compare by insertion order (FIFO for ties)
                return x.InsertionOrder.CompareTo(y.InsertionOrder);
            }
        }
    }
}
