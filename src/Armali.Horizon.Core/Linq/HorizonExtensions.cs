namespace Armali.Horizon.Core.Linq;

public static class HorizonExtensions
{
    /// <param name="source">Sequence</param>
    /// <typeparam name="T">Type</typeparam>
    extension<T>(IEnumerable<T> source)
    {
        /// <summary>
        /// Executes an action for each element
        /// in the sequence.
        /// </summary>
        /// <param name="f">Action</param>
        public void ForEach(Action<T> f)
        {
            foreach (var x in source)
                f(x);
        }

        /// <summary>
        /// Executes an action for each element in the
        /// sequence, without consuming them. Note that
        /// Peek is not a terminal operation.
        /// </summary>
        /// <param name="f">Action</param>
        /// <returns>Sequence</returns>
        public IEnumerable<T> Peek(Action<T> f)
        {
            foreach (var x in source)
            {
                f(x);
                yield return x;
            }
        }
        
        /// <summary>
        /// Returns a random element contained inside
        /// the source sequence using Reservoir Sampling
        /// to ensure a single enumeration.
        /// </summary>
        /// <returns>Element</returns>
        public T? Random()
        {
            // Se recomienda Random.Shared en versiones modernas para evitar allocaciones repetitivas
            var rnd = System.Random.Shared; 
            T? current = default;
            var count = 0;

            foreach (var element in source)
            {
                count++;
                // Reemplaza el candidato actual con probabilidad 1/count
                if (rnd.Next(count) == 0)
                {
                    current = element;
                }
            }

            return current;
        }
        
        /// <summary>
        /// Converts a sequence into a string using the supplied
        /// function, and an optional separator string.
        /// </summary>
        /// <param name="function">Element to string converter</param>
        /// <param name="separator">Separator string</param>
        /// <returns>string</returns>
        public string Stringify(Func<T, string>? function = null, string separator = "")
        {
            function ??= e => e?.ToString() ?? string.Empty;

            var str = source.Select(e => function(e))
                .Aggregate("", (result, elem) => $"{result}{separator}{elem}");
            str = str.Remove(0, separator.Length);

            return str;
        }
    }

    /// <param name="seed">Seed element</param>
    /// <typeparam name="T">Type</typeparam>
    extension<T>(T seed)
    {
        /// <summary>
        /// Generates an infinite sequence from a
        /// starting seed and a generator function.
        /// </summary>
        /// <param name="generator">Generator function</param>
        /// <returns>Sequence</returns>
        public IEnumerable<T> Generate(Func<T, T> generator)
        {
            yield return seed;

            while (true)
                yield return seed = generator(seed);
        }

        /// <summary>
        /// Converts this object into a sequence
        /// with a single element.
        /// </summary>
        /// <returns>Sequence</returns>
        public IEnumerable<T> Enumerate()
        {
            yield return seed;
        }
    }
}
