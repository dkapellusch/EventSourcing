using System.Collections.Generic;
using System.IO;

namespace EventSourcing.KSQL
{
    public static class StreamReaderExtensions
    {
        public static string ReadChunk(this StreamReader reader, string delimiter)
        {
            var chunk = new List<char>();
            var delimiters = new CircularBuffer<char>(delimiter.Length);

            while (reader.Peek() >= 0)
            {
                var character = (char) reader.Read();
                delimiters.Enqueue(character);
                if (delimiters.ToString() == delimiter || reader.EndOfStream)
                {
                    if (chunk.Count > 0)
                    {
                        chunk.Add(character);
                        return new string(chunk.ToArray());
                    }

                    continue;
                }

                chunk.Add(character);
            }

            return string.Empty;
        }

        private sealed class CircularBuffer<T> : Queue<T>
        {
            private readonly int _capacity;

            public CircularBuffer(int capacity) : base(capacity) => _capacity = capacity;

            public new void Enqueue(T item)
            {
                if (Count == _capacity) Dequeue();

                base.Enqueue(item);
            }

            public override string ToString() => string.Join("", this);
        }
    }
}