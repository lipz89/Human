using System.Linq;

namespace Human.Core
{
    class Task
    {
        private readonly IStep[] commands;

        public IStep Current { get; private set; }
        public Data CurrentData { get; internal set; }
        public Project Project { get; }

        public int CodeLineCount
        {
            get { return commands.Length; }
        }

        public int StepCount { get; private set; }

        public Task(Project project, params IStep[] commands)
        {
            this.Project = project;
            this.commands = commands;
            this.Current = this.commands.FirstOrDefault();
        }

        private Result Move(bool assertResult)
        {
            if (assertResult && this.Current is IJump jump)
            {
                this.Current = this.commands.FirstOrDefault(x => x.Label == jump.To);
                if (this.Current == null)
                {
                    return Result.Error("错误的跳转地址。");
                }
            }
            else
            {
                this.Current = this.Current.Next;
            }

            while (this.Current is EmptyStep)
            {
                this.Current = this.Current.Next;
            }

            if (this.Current != null)
                return Result.OK();
            else
                return Result.End();
        }

        public Result Run()
        {
            while (true)
            {
                var assertValue = false;
                Result result = null;
                if (this.Current is ICommand command)
                {
                    result = command.Do(this);
                    this.StepCount++;
                }
                else if (this.Current is IJump jump)
                {
                    result = jump.Assert(this);
                    if (result.Status == Status.Error)
                    {
                        return result;
                    }
                    if (result.Status == Status.Ok)
                    {
                        assertValue = true;
                        this.StepCount++;
                    }
                }

                if (result == null || result.Status == Status.End)
                {
                    return Result.OK();
                }

                if (result.Status == Status.Error)
                {
                    return result;
                }

                var moveRst = this.Move(assertValue);
                if (moveRst.Status != Status.Ok)
                {
                    return moveRst;
                }
            }
        }
    }
}