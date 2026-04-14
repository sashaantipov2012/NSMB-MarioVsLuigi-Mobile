using Quantum.Task;

namespace Quantum {
    public unsafe abstract class SystemMainThreadEntity<E> : SystemBase where E : unmanaged, IComponent {

        private TaskDelegateHandle updateTaskHandle;

        protected override TaskHandle Schedule(Frame f, TaskHandle taskHandle) {
            if (f.ComponentCount<E>() <= 0) {
                return taskHandle;
            }
            if (!updateTaskHandle.IsValid) {
                f.Context.TaskContext.RegisterDelegate(UpdateTask, $"{GetType().Name}.UpdateTask", ref updateTaskHandle);
            }
            return f.Context.TaskContext.AddMainThreadTask(updateTaskHandle, null, taskHandle);
        }

        public void UpdateTask(FrameThreadSafe f, int start, int count, void* arg) {
            Update((Frame) f);
        }

        public abstract void Update(Frame f);

    }
}