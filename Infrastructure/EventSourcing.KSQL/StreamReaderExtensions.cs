using System.Collections.Generic;
using System.IO;

namespace EventSourcing.KSQL
{
    public static class StreamReaderExtensions
    {
        public static IEnumerable<string> ReadUntil(this StreamReader reader, string delimiter)
        {
            while (reader.Peek() >= 0)
            {
                yield return reader.ReadChunk(delimiter);
            }
        }

        public static string ReadChunk(this StreamReader reader, string delimiter)
        {
            var chunk = new List<char>();
            var delimiters = new CircularBuffer<char>(delimiter.Length);
            while (reader.Peek() >= 0)
            {
                var c = (char) reader.Read();
                delimiters.Enqueue(c);
                if (delimiters.ToString() == delimiter || reader.EndOfStream)
                {
                    if (chunk.Count > 0)
                    {
                        chunk.Add(c);

                        return new string(chunk.ToArray());
                    }

                    continue;
                }

                chunk.Add(c);
            }

            return string.Empty;
        }

        private class CircularBuffer<T> : Queue<T>
        {
            private readonly int _capacity;

            public CircularBuffer(int capacity)
                : base(capacity)
            {
                _capacity = capacity;
            }

            public new void Enqueue(T item)
            {
                if (Count == _capacity) Dequeue();

                base.Enqueue(item);
            }

            public override string ToString()
            {
                return string.Join("", this);
            }
        }
    }
}