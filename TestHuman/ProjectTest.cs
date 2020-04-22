﻿using System;
using System.Collections.Generic;
using System.Linq;
using Human.Core;

namespace TestHuman
{
    abstract class ProjectTest : Project
    {
        protected ProjectTest(byte memorySize) : base(memorySize)
        {
        }

        protected abstract StepGen CreateGen();
        protected abstract Input[] CreateInputs();

        public void Test()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"\r\n测试 { Name}");
            Console.WriteLine($"\t{Description} ");
            var steps = CreateGen();
            var inputs = CreateInputs();
            int sum = 0;
            foreach (var input in inputs)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"输入 ({input.List.Count})：\r\n\t" + string.Join(", ", input.List));
                ITaskResult result = Run(steps, input);
                Console.WriteLine($"输出 ({result.Output.Count})：\r\n\t" + string.Join(", ", result.Output));
                Console.WriteLine($"代码行数：{result.CodeLineCount}");
                Console.WriteLine($"执行步骤：{result.StepCount}");
                if (result.Status != Status.Ok)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error:{result.Message}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("执行成功。");
                }
                sum += result.StepCount;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"平均执行步骤：{sum / inputs.Length}");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(steps);
        }
    }

    class Project1 : ProjectTest
    {
        public Project1() : base(0)
        {
        }

        public override string Name { get; } = "原样输出";
        public override string Description { get; } = "将INBOX中的所有内容放到OUTBOX";

        protected override IList<Data> GetExpect(Input input)
        {
            return input.List;
        }

        protected override StepGen CreateGen()
        {
            return new StepGen()
                .Input("a")
                .Output()
                .Jump("a");
        }

        protected override Input[] CreateInputs()
        {
            return new Input[]
            {
                new Input(1,2,3),
                new Input(-1,0,-2,3,0,20)
            };
        }
    }

    class Project2 : ProjectTest
    {
        public Project2() : base(5)
        {
        }

        public override string Name { get; } = "成对倒转";
        public override string Description { get; } = "每次从INBOX取两个数据，翻转顺序后放到OUTBOX，重复步骤直到INBOX没有数据";

        protected override IList<Data> GetExpect(Input input)
        {
            var output = new List<Data>();
            Data temp = null;
            foreach (var item in input.List)
            {
                if (temp == null)
                {
                    temp = item;
                }
                else
                {
                    output.Add(item);
                    output.Add(temp);
                    temp = null;
                }
            }

            return output;
        }

        protected override StepGen CreateGen()
        {
            return new StepGen()
                .Input("a")
                .CopyTo(1)
                .Input()
                .Output()
                .CopyFrom(1)
                .Output()
                .Jump("a");
        }

        protected override Input[] CreateInputs()
        {
            return new Input[]
            {
                new Input(1,2,3,4),
                new Input(-1,0).Add('A','B','C').Add(20)
            };
        }
    }

    class Project3 : ProjectTest
    {
        public Project3() : base(0)
        {
        }

        public override string Name { get; } = "零保护行动";
        public override string Description { get; } = "只把INBOX中的所有0放到OUTBOX";

        protected override IList<Data> GetExpect(Input input)
        {
            return input.List.Where(x => x is Data<int> i && i.Value == 0).ToList();
        }

        protected override StepGen CreateGen()
        {
            return new StepGen()
                .Jump("b")
                .Output("a")
                .Input("b")
                .JumpIfZero("a")
                .Jump("b");
        }

        protected override Input[] CreateInputs()
        {
            return new Input[]
            {
                new Input(1,0,0,4),
                new Input(0,2,0,0),
                new Input(1,0,0,0),
                new Input(-1,0).Add('A','B','C').Add(20)
            };
        }
    }

    class Project4 : ProjectTest
    {
        public Project4() : base(4)
        {
        }

        public override string Name { get; } = "计时器";
        public override string Description { get; } = "对于INBOX中的每一个数，都把他和0之间的每一个数(包括他自身和0)放到OUTBOX中";

        protected override IList<Data> GetExpect(Input input)
        {
            var output = new List<Data>();
            foreach (var item in input.List)
            {
                if (item is Data<int> i)
                {
                    var v = i.Value;
                    if (v > 0)
                    {
                        for (int j = v; j >= 0; j--)
                        {
                            output.Add(new Data<int>(j));
                        }
                    }
                    else if (v < 0)
                    {
                        for (int j = v; j <= 0; j++)
                        {
                            output.Add(new Data<int>(j));
                        }
                    }
                    else
                    {
                        output.Add(new Data<int>(0));
                    }
                }
            }

            return output;
        }

        protected override StepGen CreateGen()
        {
            return new StepGen()
                .Jump("d")
                .Output("a")
                .Input("d")
                .JumpIfZero("a")
                .CopyTo(0)
                .JumpIfNegative("f")
                .Output("e")
                .BumpSub(0)
                .JumpIfZero("a")
                .Jump("e")
                .Output("f")
                .BumpAdd(0)
                .JumpIfZero("a")
                .Jump("f");
        }

        protected override Input[] CreateInputs()
        {
            return new Input[]
            {
                new Input(5,-3,0,8,2,3,4),
                new Input(9,-6,0,3),
                new Input(9,-4,0,9),
                new Input(4,-5,0,-8),
                new Input(5,-5,0,9),
                new Input(6,-8,0,6),
                new Input(4,-4,0,7),
            };
        }
    }
}