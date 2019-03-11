using System.Collections.Generic;
using System.Linq;

namespace Human.Core
{
    public abstract class Project
    {
        private readonly byte memorySize;

        protected Project(byte memorySize)
        {
            this.memorySize = memorySize;
        }

        internal Input Input { get; private set; }
        internal Output Output { get; private set; }
        internal Queue<Data> Expect { get; private set; }
        internal Memory Memory { get; private set; }

        public abstract string Name { get; }
        public abstract string Description { get; }

        private void Init(Input input)
        {
            this.Input = input;
            this.Expect = new Queue<Data>(GetExpect(input));
            this.Output = new Output();
            this.Memory = new Memory(memorySize);
        }

        protected abstract IList<Data> GetExpect(Input input);

        public ITaskResult Run(StepGen stepGen, Input input)
        {
            this.Init(input);
            var task = new Task(this, stepGen.ToArray());
            var rst = task.Run();
            if (rst.Status == Status.Error)
            {
                return TaskResult.Fail(rst.Message, task.CodeLineCount, task.StepCount, Input.List, Output.List);
            }

            //if (Input.Any())
            //{
            //    return Result.Error("还有数据没有处理。");
            //}

            var count = Expect.Count;
            if (count > 0)
            {
                return TaskResult.Fail($"还有 {count} 个数据没有输出。", task.CodeLineCount, task.StepCount, Input.List, Output.List);
            }

            return TaskResult.OK(task.CodeLineCount, task.StepCount, Input.List, Output.List);
        }
    }

    public class Input
    {
        private readonly List<Data> list = new List<Data>();
        private int index;

        internal bool TryGet(out Data data)
        {
            if (this.Any())
            {
                data = this.list[index];
                this.index++;
                return true;
            }

            data = Data.Empty;
            return false;
        }

        internal bool Any()
        {
            return this.index < this.list.Count;
        }

        public List<Data> List
        {
            get { return this.list.ToList(); }
        }

        public Input Add(int value)
        {
            this.list.Add(new Data<int>(value));
            return this;
        }

        public Input Add(char value)
        {
            this.list.Add(new Data<char>(value));
            return this;
        }
    }

    class Output
    {
        private readonly List<Data> list = new List<Data>();

        public void Push(Data item)
        {
            this.list.Add(item);
        }

        public List<Data> List
        {
            get { return this.list.ToList(); }
        }
    }

    class Memory
    {
        private readonly Data[] buffer;

        public Memory(byte count)
        {
            this.buffer = new Data[count];
        }

        public bool HasIndex(int index)
        {
            return index < this.buffer.Length;
        }

        public Data Get(int index)
        {
            return buffer[index];
        }

        public void Set(int index, Data data)
        {
            this.buffer[index] = data;
        }
    }
}