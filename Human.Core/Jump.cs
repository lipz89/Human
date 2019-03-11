namespace Human.Core
{
    interface IJump
    {
        string To { get; set; }
        Result Assert(Task task);
    }

    class Jump : Step, IJump
    {
        public string To { get; set; }

        public Result Assert(Task task)
        {
            return Result.OK();
        }

        protected override string String()
        {
            return StepConst.JUMP + StepConst.TAB + To;
        }
    }

    class JumpIfZero : Step, IJump
    {
        public string To { get; set; }

        public Result Assert(Task task)
        {
            if (task.CurrentData == null)
            {
                return Result.Error("没有数据可供比较。");
            }

            if (task.CurrentData is Data<int> i && i.Value == 0)
            {
                return Result.OK();
            }

            return Result.Fail();
        }

        protected override string String()
        {
            return StepConst.JUMPZ + StepConst.TAB + To;
        }
    }

    class JumpIfNegative : Step, IJump
    {
        public string To { get; set; }

        public Result Assert(Task task)
        {
            if (task.CurrentData == null)
            {
                return Result.Error("没有数据可供比较。");
            }

            if (task.CurrentData is Data<int> i && i.Value < 0)
            {
                return Result.OK();
            }

            return Result.Fail();
        }

        protected override string String()
        {
            return StepConst.JUMPN + StepConst.TAB + To;
        }
    }
}