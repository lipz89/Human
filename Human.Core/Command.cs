namespace Human.Core
{
    interface ICommand
    {
        Result Do(Task task);
    }

    class In : Step, ICommand
    {
        public Result Do(Task task)
        {
            if (task.Project.Input.TryGet(out Data item))
            {
                task.CurrentData = item;
                return Result.OK();
            }

            return Result.End();
        }

        protected override string String()
        {
            return StepConst.INBOX;
        }
    }

    class Out : Step, ICommand
    {
        public Result Do(Task task)
        {
            if (task.CurrentData != null && !task.CurrentData.IsEmpty)
            {
                if (task.Project.Expect.Count == 0)
                {
                    return Result.Error("不需要输出数据了。");
                }

                var expect = task.Project.Expect.Dequeue();
                if (!expect.Equals(task.CurrentData))
                {
                    return Result.Error($"应该输出 {expect} ，而不是 {task.CurrentData} 。");
                }

                task.Project.Output.Push(task.CurrentData);
                return Result.OK();
            }

            return Result.Error("没有数据可以输出。");
        }

        protected override string String()
        {
            return StepConst.OUTBOX;
        }
    }

    class OpMemoryCommand : Step
    {
        public int MemoryIndex { get; set; }
    }

    class CopyFrom : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (!task.Project.Memory.HasIndex(MemoryIndex))
            {
                return Result.Error($"试图访问不存在的内存地址 {MemoryIndex}。");
            }

            task.CurrentData = task.Project.Memory.Get(MemoryIndex);
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.COPYFROM + StepConst.TAB + MemoryIndex;
        }
    }

    class CopyTo : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (!task.Project.Memory.HasIndex(MemoryIndex))
            {
                return Result.Error($"试图访问不存在的内存地址 {MemoryIndex}。");
            }

            task.Project.Memory.Set(MemoryIndex, task.CurrentData);
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.COPYTO + StepConst.TAB + MemoryIndex;
        }
    }

    class Add : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (!task.Project.Memory.HasIndex(MemoryIndex))
            {
                return Result.Error($"试图访问不存在的内存地址 {MemoryIndex}。");
            }

            var temp = task.Project.Memory.Get(MemoryIndex);
            if (!(temp is Data<int> tempint))
            {
                return Result.Error($"内存 {MemoryIndex} 没有数据可以进行加法运算。");
            }

            if (!(task.CurrentData is Data<int> curint))
            {
                return Result.Error("当前没有数据可以进行加法运算。");
            }

            task.CurrentData = new Data<int>(curint.Value + tempint.Value);
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.ADD + StepConst.TAB + MemoryIndex;
        }
    }

    class Sub : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (!task.Project.Memory.HasIndex(MemoryIndex))
            {
                return Result.Error($"试图访问不存在的内存地址 {MemoryIndex}。");
            }

            var temp = task.Project.Memory.Get(MemoryIndex);
            if (!(temp is Data<int> tempint))
            {
                return Result.Error($"内存 {MemoryIndex} 没有数据可以进行减法运算。");
            }

            if (!(task.CurrentData is Data<int> curint))
            {
                return Result.Error("当前没有数据可以进行减法运算。");
            }

            task.CurrentData = new Data<int>(curint.Value - tempint.Value);
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.SUB + StepConst.TAB + MemoryIndex;
        }
    }

    class BumpAdd : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (!task.Project.Memory.HasIndex(MemoryIndex))
            {
                return Result.Error($"试图访问不存在的内存地址 {MemoryIndex}。");
            }

            var temp = task.Project.Memory.Get(MemoryIndex);
            if (!(temp is Data<int> tempint))
            {
                return Result.Error($"内存 {MemoryIndex} 没有数据可以进行自加运算。");
            }

            var data = new Data<int>(tempint.Value + 1);
            task.Project.Memory.Set(MemoryIndex, data);
            task.CurrentData = data;
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.BUMPUP + StepConst.TAB + MemoryIndex;
        }
    }

    class BumpSub : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (!task.Project.Memory.HasIndex(MemoryIndex))
            {
                return Result.Error($"试图访问不存在的内存地址 {MemoryIndex}。");
            }

            var temp = task.Project.Memory.Get(MemoryIndex);
            if (!(temp is Data<int> tempint))
            {
                return Result.Error($"内存 {MemoryIndex} 没有数据可以进行自减运算。");
            }

            var data = new Data<int>(tempint.Value - 1);
            task.Project.Memory.Set(MemoryIndex, data);
            task.CurrentData = data;
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.BUMPDN + StepConst.TAB + MemoryIndex;
        }
    }
}