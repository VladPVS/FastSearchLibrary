namespace FastSearchLibrary
{
    /// <summary>
    /// Specifies where event handlers are executed.
    /// </summary>
    public enum ExecuteHandlers
    {
        /// <summary>
        /// To execute event handlers in current task. 
        /// </summary>
        InCurrentTask = 0,

        /// <summary>
        /// To execute event handlers in new task.
        /// </summary>
        InNewTask = 1
    }
}
