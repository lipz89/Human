using System;
using System.Collections.Generic;
using System.Linq;

namespace Human.Core
{
    interface IStep
    {
        IStep Next { get; }
        string Label { get; }
        //string Comment { get; }
    }

    abstract class Step : IStep
    {
        public IStep Next { get; set; }
        public string Label { get; set; }
        public string Comment { get; set; }

        protected virtual string String()
        {
            return string.Empty;
        }

        public override string ToString()
        {
            var str = string.Empty;
            if (Label != null)
            {
                str += Label + StepConst.COLON + StepConst.NEWLINE;
            }

            if (!string.IsNullOrWhiteSpace(this.Comment))
            {
                str += StepConst.COMMENT + StepConst.SPACE + this.Comment + StepConst.NEWLINE;
            }

            str += StepConst.TAB + this.String();
            return str;
        }
    }

    class EmptyStep : Step
    {

    }

    public class StepGen
    {
        private readonly List<IStep> steps = new List<IStep>();

        private StepGen Add(IStep step, string comment = null)
        {
            var last = this.steps.LastOrDefault();
            if (last != null)
            {
                ((Step)last).Next = step;
            }

            if (!string.IsNullOrWhiteSpace(comment))
            {
                ((Step)step).Comment = comment;
            }

            this.steps.Add(step);
            return this;
        }

        public StepGen Empty(string label = null)
        {
            return this.Add(new EmptyStep { Label = label });
        }

        public StepGen Jump(string to, string label = null, string comment = null)
        {
            return this.Add(new Jump { To = to, Label = label, Comment = comment });
        }

        public StepGen JumpIfZero(string to, string label = null, string comment = null)
        {
            return this.Add(new JumpIfZero { To = to, Label = label, Comment = comment });
        }

        public StepGen JumpIfNegative(string to, string label = null, string comment = null)
        {
            return this.Add(new JumpIfNegative { To = to, Label = label, Comment = comment });
        }

        public StepGen Input(string label = null, string comment = null)
        {
            return this.Add(new In { Label = label, Comment = comment });
        }

        public StepGen Output(string label = null, string comment = null)
        {
            return this.Add(new Out { Label = label, Comment = comment });
        }

        public StepGen Add(int index, string label = null, string comment = null)
        {
            return this.Add(new Add { MemoryIndex = index, Label = label, Comment = comment });
        }

        public StepGen Sub(int index, string label = null, string comment = null)
        {
            return this.Add(new Sub { MemoryIndex = index, Label = label, Comment = comment });
        }

        public StepGen BumpAdd(int index, string label = null, string comment = null)
        {
            return this.Add(new BumpAdd { MemoryIndex = index, Label = label, Comment = comment });
        }

        public StepGen BumpSub(int index, string label = null, string comment = null)
        {
            return this.Add(new BumpSub { MemoryIndex = index, Label = label, Comment = comment });
        }

        public StepGen CopyFrom(int index, string label = null, string comment = null)
        {
            return this.Add(new CopyFrom { MemoryIndex = index, Label = label, Comment = comment });
        }

        public StepGen CopyTo(int index, string label = null, string comment = null)
        {
            return this.Add(new CopyTo { MemoryIndex = index, Label = label, Comment = comment });
        }

        private StepGen SkipEmpty()
        {
            IStep last = null;
            IStep step = this.steps.FirstOrDefault();
            while (step != null)
            {
                if (step is EmptyStep empty)
                {
                    var nnext = (Step)empty.Next;
                    var lbl = empty.Label;
                    if (string.IsNullOrWhiteSpace(nnext.Label))
                    {
                        nnext.Label = lbl;
                    }

                    var js = this.steps.OfType<IJump>().Where(x => x.To == lbl);
                    foreach (var jump in js)
                    {
                        jump.To = nnext.Label;
                    }

                    this.steps.Remove(step);
                    if (last != null) ((Step)last).Next = nnext;
                }
                else
                {
                    last = step;
                }

                step = step.Next;
            }
            return this;
        }
        internal IStep[] ToArray()
        {
            return this.steps.ToArray();
        }
        public override string ToString()
        {
            return StepConst.COMMENT + StepConst.SPACE + StepConst.MARK + StepConst.SPACE + StepConst.COMMENT + StepConst.NEWLINE
                   + string.Join("\r\n", steps);
        }
        public static StepGen Parse(string text)
        {
            var gen = new StepGen();
            var labels = new List<string>();
            //string comment = string.EmptyStep;
            var lines = text.Split(StepConst.NEWLINE.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith(StepConst.COMMENT))
                {
                    //var comt = line.Trim(StepConst.COMMENT.ToCharArray()).Trim();
                    //if (!comt.Equals(StepConst.MARK, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    comment += comt + StepConst.SPACE + StepConst.COMMENT;
                    //}
                    continue;
                }

                if (line.StartsWith(StepConst.DEFINE, StringComparison.OrdinalIgnoreCase))
                {
                    while (!line.EndsWith(StepConst.DEFINEEND))
                    {
                        i++;
                        line = lines[i].Trim();
                    }
                    continue;
                }

                var index = line.IndexOf(StepConst.COMMENT, StringComparison.Ordinal);
                if (index > 0)
                {
                    line = line.Substring(0, index).Trim();
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.EndsWith(StepConst.COLON))
                {
                    labels.AddRange(line.Split(StepConst.COLON.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    if (labels.Any())
                    {
                        foreach (var label in labels)
                        {
                            gen.Empty(label);
                        }
                        labels.Clear();
                    }

                    var step = ParseStep(line);
                    if (step == null)
                    {
                        return null;
                    }

                    //step.Comment = comment.Trim(StepConst.COMMENT.ToCharArray()).Trim();
                    gen.Add(step);
                    //comment = string.EmptyStep;
                }
            }

            return gen.SkipEmpty();
        }

        private static IStep ParseStep(string line)
        {
            if (line.StartsWith(StepConst.INBOX, StringComparison.OrdinalIgnoreCase))
            {
                return new In();
            }

            if (line.StartsWith(StepConst.OUTBOX, StringComparison.OrdinalIgnoreCase))
            {
                return new Out();
            }

            var sps = line.Split(StepConst.SPLITS.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (sps.Length < 2)
            {
                return null;
            }

            var cmd = sps[0];
            var arg = sps[1];
            if (cmd.StartsWith(StepConst.JUMP, StringComparison.OrdinalIgnoreCase))
            {
                if (cmd.Equals(StepConst.JUMPN, StringComparison.OrdinalIgnoreCase))
                {
                    return new JumpIfNegative { To = arg };
                }

                if (cmd.Equals(StepConst.JUMPZ, StringComparison.OrdinalIgnoreCase))
                {
                    return new JumpIfZero { To = arg };
                }

                if (cmd.Equals(StepConst.JUMP, StringComparison.OrdinalIgnoreCase))
                {
                    return new Jump { To = arg };
                }

                return null;
            }

            var usePointer = false;
            if (arg.StartsWith(StepConst.POINTSTART) && arg.EndsWith(StepConst.POINTEDN))
            {
                usePointer = true;
                arg = arg.Replace(StepConst.POINTSTART, "").Replace(StepConst.POINTEDN, "").Trim();
            }

            if (int.TryParse(arg, out int mindex))
            {
                if (cmd.Equals(StepConst.COPYFROM, StringComparison.OrdinalIgnoreCase))
                {
                    return new CopyFrom { MemoryIndex = mindex, UsePointer = usePointer };
                }

                if (cmd.Equals(StepConst.COPYTO, StringComparison.OrdinalIgnoreCase))
                {
                    return new CopyTo { MemoryIndex = mindex, UsePointer = usePointer };
                }

                if (cmd.Equals(StepConst.ADD, StringComparison.OrdinalIgnoreCase))
                {
                    return new Add { MemoryIndex = mindex, UsePointer = usePointer };
                }

                if (cmd.Equals(StepConst.SUB, StringComparison.OrdinalIgnoreCase))
                {
                    return new Sub { MemoryIndex = mindex, UsePointer = usePointer };
                }

                if (cmd.Equals(StepConst.BUMPUP, StringComparison.OrdinalIgnoreCase))
                {
                    return new BumpAdd { MemoryIndex = mindex, UsePointer = usePointer };
                }

                if (cmd.Equals(StepConst.BUMPDN, StringComparison.OrdinalIgnoreCase))
                {
                    return new BumpSub { MemoryIndex = mindex, UsePointer = usePointer };
                }
            }

            return null;
        }
    }

    static class StepConst
    {
        public const string COMMENT = "--";
        public const string MARK = "BY CUSTOM HUMAN MODEL";
        public const string NEWLINE = "\r\n";
        public const string TAB = "\t";
        public const string COLON = ":";
        public const string SPACE = " ";
        public const string SPLITS = SPACE + TAB;

        public const string JUMP = "JUMP";
        public const string JUMPZ = "JUMPZ";
        public const string JUMPN = "JUMPN";

        public const string INBOX = "INBOX";
        public const string OUTBOX = "OUTBOX";
        public const string COPYFROM = "COPYFROM";
        public const string COPYTO = "COPYTO";
        public const string ADD = "ADD";
        public const string SUB = "SUB";
        public const string BUMPUP = "BUMPUP";
        public const string BUMPDN = "BUMPDN";

        public const string DEFINE = "DEFINE";
        public const string DEFINEEND = ";";

        public const string POINTSTART = "[";
        public const string POINTEDN = "]";
    }
}