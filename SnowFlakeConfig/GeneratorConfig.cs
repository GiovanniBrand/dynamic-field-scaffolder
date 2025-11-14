namespace Dynamic.Scaffolder.SnowFlakeConfig
{
    public class GeneratorConfig
    {
        public int Id { get; set; }
        public DateTime Epoch { get; set; }
        public int TimestampBits { get; set; }
        public int GeneratorIdBits { get; set; }
        public int SequenceBits { get; set; }
    }
}
