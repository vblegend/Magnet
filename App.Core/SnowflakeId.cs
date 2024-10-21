using System;

namespace App.Core
{
    public readonly struct SnowflakeId : IEquatable<SnowflakeId>
    {
        private readonly Int64 m_value;

        private SnowflakeId(Int64 value)
        {
            this.m_value = value;
        }

        public override string ToString()
        {
            return this.m_value.ToString("X").ToUpper();
        }

        public Boolean IsEmpty
        {
            get
            {
                return (Byte)((this.m_value >> 60) & 0xF) != 7;
            }
        }

        public Int32 type
        {
            get
            {
                return (Int32)((this.m_value >> 48) & 0x0FFF);
            }
        }

        public Int32 Timestamp
        {
            get
            {
                return (Int32)((this.m_value >> 16) & 0xFFFFFFFF);
            }
        }

        public DateTime UTCTime
        {
            get
            {
                return STARTDATE_DEFINE.AddSeconds((this.m_value >> 16) & 0xFFFFFFFF);
            }
        }

        public DateTime LocalTime
        {
            get
            {
                return TimeZoneInfo.ConvertTimeFromUtc(STARTDATE_DEFINE.AddSeconds((this.m_value >> 16) & 0xFFFFFFFF), TimeZoneInfo.Local);
            }
        }

        public UInt16 Sequence
        {
            get
            {
                return (UInt16)(this.m_value & 0xFFFF);
            }
        }


        public Int64 Value
        {
            get
            {
                return this.m_value;
            }
        }

        public static bool operator ==(SnowflakeId s1, SnowflakeId s2)
        {
            return s1.m_value == s2.m_value;
        }


        public static bool operator !=(SnowflakeId s1, SnowflakeId s2)
        {
            return s1.m_value != s2.m_value;
        }

        public bool Equals(SnowflakeId other)
        {
            return other.m_value == this.m_value;
        }

        public override int GetHashCode()
        {
            return unchecked((int)((long)m_value)) ^ (int)(m_value >> 32);
        }

        public override bool Equals(object? obj)
        {
            if (obj is SnowflakeId id)
            {
                return id.m_value == this.m_value;
            }
            return false;
        }

        public static implicit operator SnowflakeId(String value)
        {
            long result = Convert.ToInt64(value, 16);
            return new SnowflakeId(result);
        }

        public static implicit operator SnowflakeId(Int64 value)
        {
            return new SnowflakeId(value);
        }




        #region Static 


        private static readonly DateTime STARTDATE_DEFINE = new DateTime(2024, 1, 1, 12, 34, 56, DateTimeKind.Utc);
        private static long _lastTimestamp = -1L;
        private static long _sequence = 1L;
        private static readonly object _lock = new object();


        public static SnowflakeId Generate(Int32 type)
        {
            if (type < 0 || type > 0xFFF)
            {
                throw new ArgumentOutOfRangeException(nameof(type), $"Type must be between 0 and 0xFFF");
            }
            lock (_lock)
            {
                var timestamp = GetCurrentTimestamp();
                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & 0xFFFF;
                    if (_sequence == 0L)
                    {
                        timestamp = WaitNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    _sequence = 1L;
                }
                _lastTimestamp = timestamp;
                long id =
                    ((long)0x7 << 60) |
                    ((long)(type & 0xFFF) << 48) |
                    (timestamp << 16) |
                    _sequence;
                return new SnowflakeId(id);
            }


        }


        private static long GetCurrentTimestamp()
        {
            return (long)(DateTime.UtcNow - STARTDATE_DEFINE).TotalSeconds;
        }

        private static long WaitNextMillis(long lastTimestamp)
        {
            var timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }

        #endregion

    }
}
