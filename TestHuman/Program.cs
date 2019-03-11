using System;
using Human.Core;

namespace TestHuman
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "人力资源机器模拟";
            //TestGen();
            //Test<Project1>();
            //Test<Project2>();
            //Test<Project3>();
            Test<Project4>();
            Console.Read();
        }

        static void Test<T>() where T : ProjectTest, new()
        {
            new T().Test();
        }

        static void TestGen()
        {
            string str = @"-- HUMAN RESOURCE MACHINE PROGRAM --

    JUMP     b
a:
    OUTBOX  
b:
c:
    INBOX   
    JUMPZ    a
    JUMP     c
";
            var gen = StepGen.Parse(str);
            Console.WriteLine(gen);
        }
    }
}
