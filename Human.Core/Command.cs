using System.Xml;

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
        public bool UsePointer { get; set; }

        protected string IndexString
        {
            get
            {
                if (!UsePointer)
                {
                    return MemoryIndex.ToString();
                }
                return StepConst.POINTSTART + MemoryIndex + StepConst.POINTEDN;
            }
        }

        protected bool TryVisitMemory(Task task, out int index, out Result error)
        {
            index = MemoryIndex;
            error = null;
            if (!task.Project.Memory.HasIndex(MemoryIndex))
            {
                error = Result.Error($"试图访问不存在的内存地址 {MemoryIndex}。");
                return false;
            }

            if (!UsePointer)
            {
                return true;
            }

            var data = task.Project.Memory.Get(MemoryIndex);
            if (data is Data<int> dint)
            {
                index = dint.Value;
                if (task.Project.Memory.HasIndex(index))
                {
                    return true;
                }
                error = Result.Error($"试图访问不存在的内存地址 {index}。");
                return false;
            }

            error = Result.Error($"试图访问的内存地址 {index} 不正确。");
            return false;
        }
    }

    class CopyFrom : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (TryVisitMemory(task, out var index, out var error))
            {
                return error;
            }

            task.CurrentData = task.Project.Memory.Get(index);
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.COPYFROM + StepConst.TAB + IndexString;
        }
    }

    class CopyTo : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (TryVisitMemory(task, out var index, out var error))
            {
                return error;
            }

            task.Project.Memory.Set(index, task.CurrentData);
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.COPYTO + StepConst.TAB + IndexString;
        }
    }

    class Add : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (TryVisitMemory(task, out var index, out var error))
            {
                return error;
            }

            var temp = task.Project.Memory.Get(index);
            if (!(temp is Data<int> tempint))
            {
                return Result.Error($"内存 {index} 没有数据可以进行加法运算。");
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
            return StepConst.ADD + StepConst.TAB + IndexString;
        }
    }

    class Sub : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (TryVisitMemory(task, out var index, out var error))
            {
                return error;
            }

            var temp = task.Project.Memory.Get(index);
            if (!(temp is Data<int> tempint))
            {
                return Result.Error($"内存 {index} 没有数据可以进行减法运算。");
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
            return StepConst.SUB + StepConst.TAB + IndexString;
        }
    }

    class BumpAdd : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (TryVisitMemory(task, out var index, out var error))
            {
                return error;
            }

            var temp = task.Project.Memory.Get(index);
            if (!(temp is Data<int> tempint))
            {
                return Result.Error($"内存 {index} 没有数据可以进行自加运算。");
            }

            var data = new Data<int>(tempint.Value + 1);
            task.Project.Memory.Set(index, data);
            task.CurrentData = data;
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.BUMPUP + StepConst.TAB + IndexString;
        }
    }

    class BumpSub : OpMemoryCommand, ICommand
    {
        public Result Do(Task task)
        {
            if (TryVisitMemory(task, out var index, out var error))
            {
                return error;
            }

            var temp = task.Project.Memory.Get(index);
            if (!(temp is Data<int> tempint))
            {
                return Result.Error($"内存 {index} 没有数据可以进行自减运算。");
            }

            var data = new Data<int>(tempint.Value - 1);
            task.Project.Memory.Set(index, data);
            task.CurrentData = data;
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.BUMPDN + StepConst.TAB + IndexString;
        }
    }
}