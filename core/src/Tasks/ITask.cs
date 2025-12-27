using System;

namespace ArkBotFramework.Tasks
{
    /// <summary>
    /// Interface for all bot tasks.
    /// Defines the contract for task execution, priority, and scheduling.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Task name for identification and logging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Flag indicating whether this task has run at least once.
        /// Used to determine initial scheduling behavior.
        /// </summary>
        bool HasRunBefore { get; set; }

        /// <summary>
        /// Execute the task's main logic.
        /// This method contains all the work the task performs.
        /// </summary>
        void Execute();

        /// <summary>
        /// Get the priority level for this task.
        /// Lower numbers indicate higher priority.
        /// </summary>
        /// <returns>Priority level (1=Low, 2=High, 3=Medium, 4=Ultra)</returns>
        int GetPriorityLevel();

        /// <summary>
        /// Get the delay in seconds before this task should run again.
        /// </summary>
        /// <returns>Delay in seconds</returns>
        double GetRequeueDelay();
    }

    /// <summary>
    /// Abstract base class for bot tasks.
    /// Provides common functionality and enforces task contract.
    /// </summary>
    public abstract class BaseTask : ITask
    {
        /// <inheritdoc/>
        public string Name { get; protected set; }

        /// <inheritdoc/>
        public bool HasRunBefore { get; set; }

        protected BaseTask(string name)
        {
            Name = name;
            HasRunBefore = false;
        }

        /// <inheritdoc/>
        public abstract void Execute();

        /// <inheritdoc/>
        public abstract int GetPriorityLevel();

        /// <inheritdoc/>
        public abstract double GetRequeueDelay();

        /// <summary>
        /// Mark this task as having been run.
        /// </summary>
        public void MarkAsRun()
        {
            HasRunBefore = true;
        }
    }
}
