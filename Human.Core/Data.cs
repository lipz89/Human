namespace Human.Core
{
    public abstract class Data
    {
        protected Data(bool isEmpty)
        {
            IsEmpty = isEmpty;
        }

        public bool IsEmpty { get; }
        public static Data Empty { get; } = new Empty();
    }

    public class Data<T> : Data where T : struct
    {
        public Data(T value) : base(false)
        {
            Value = value;
        }

        public T Value { get; }

        public override bool Equals(object obj)
        {
            if (this.GetType() != obj?.GetType())
            {
                return false;
            }

            return this.Value.Equals(((Data<T>)obj).Value);
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }

    class Empty : Data
    {
        public Empty() : base(true)
        {
        }

        public override bool Equals(object obj)
        {
            return this.GetType() == obj?.GetType();
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "[Empty]";
        }
    }
}