namespace Dynamic.Scaffolder.SnowFlakeConfig
{
    public class SnowflakeGenerator : ISnowflakeGenerator
    {
        private readonly long _workerId;
        private readonly long _epoch;
        private readonly long _maxWorkerId;
        private readonly long _maxSequence;
        private readonly int _timestampShift;
        private readonly int _workerIdShift;
        private long _sequence = 0L;
        private long _lastTimestamp = -1L;
        private readonly object _lock = new object();

        public SnowflakeGenerator(GeneratorConfig config)
        {
            var generatorIdBits = config.GeneratorIdBits;
            var sequenceBits = config.SequenceBits;
            _maxWorkerId = (1L << generatorIdBits) - 1;
            _maxSequence = (1L << sequenceBits) - 1;
            _workerIdShift = sequenceBits;
            _timestampShift = generatorIdBits + sequenceBits;
            if (config.Id < 0 || config.Id > _maxWorkerId)
            {
                throw new ArgumentException($"Worker ID must be between 0 and {_maxWorkerId}");
            }
            _workerId = config.Id;
            _epoch = new DateTimeOffset(config.Epoch).ToUnixTimeMilliseconds();
        }

        public long NextId()
        {
            lock (_lock)
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (timestamp < _lastTimestamp) throw new InvalidOperationException("Clock moved backwards.");
                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & _maxSequence;
                    if (_sequence == 0)
                    {
                        while ((timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) <= _lastTimestamp) { Thread.SpinWait(1); }
                    }
                }
                else
                {
                    _sequence = 0;
                }
                _lastTimestamp = timestamp;
                return ((timestamp - _epoch) << _timestampShift) | (_workerId << _workerIdShift) | _sequence;
            }
        }
    }
}
