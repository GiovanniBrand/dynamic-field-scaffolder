namespace Dynamic.Scaffolder.Helpers
{
    public static class StringHelper
    {
        public static string RandomString()
        {
            Random random = RandomProvider.GetThreadRandom();
            return new string((from s in Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8)
                               select s[random.Next(s.Length - 1)]).ToArray());
        }

        public static class RandomProvider
        {
            private static int seed = Environment.TickCount;

            private static ThreadLocal<Random> randomWrapper = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

            public static Random GetThreadRandom()
            {
                return randomWrapper.Value;
            }
        }
    }
}
