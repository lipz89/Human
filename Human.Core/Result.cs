using System.Collections.Generic;

namespace Human.Core
{
    public interface IResult
    {
        string Message { get; }
        Status Status { get; }
    }
    public class Result : IResult
    {
        protected Result()
        {

        }

        public Status Status { get; protected set; }
        public string Message { get; protected set; }

        public static Result OK()
        {
            return new Result() { Status = Status.Ok };
        }

        public static Result End()
        {
            return new Result() { Status = Status.End };
        }

        public static Result Fail()
        {
            return new Result() { Status = Status.Fail };
        }

        public static Result Error(string message)
        {
            return new Result() { Status = Status.Error, Message = message };
        }
    }

    public interface ITaskResult : IResult
    {
        int CodeLineCount { get; }
        int StepCount { get; }
        IList<Data> Input { get; }
        IList<Data> Output { get; }
    }

    class TaskResult : Result, ITaskResult
    {
        private TaskResult()
        {

        }
        public int CodeLineCount { get; private set; }
        public int StepCount { get; private set; }
        public IList<Data> Input { get; private set; }
        public IList<Data> Output { get; private set; }

        public static TaskResult OK(int codeLine, int stepCount, IList<Data> input, IList<Data> output)
        {
            return new TaskResult
            {
                Status = Status.Ok,
                CodeLineCount = codeLine,
                StepCount = stepCount,
                Input = input,
                Output = output
            };
        }
        public static TaskResult Fail(string message, int codeLine, int stepCount, IList<Data> input, IList<Data> output)
        {
            return new TaskResult
            {
                Status = Status.Fail,
                Message = message,
                CodeLineCount = codeLine,
                StepCount = stepCount,
                Input = input,
                Output = output
            };
        }
    }

    public enum Status
    {
        Ok,
        End,
        Fail,
        Error,
    }
}