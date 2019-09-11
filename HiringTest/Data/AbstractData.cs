using System;
using System.Collections.Generic;
using System.IO;
using InterlockLedger.Tags;

namespace HiringTest
{
    public abstract class AbstractData<T> : IData<T> where T : AbstractData<T>, new()
    {
        public Payload<T> AsPayload => new Payload<T>(TagId, (T)this);
        public abstract ulong TagId { get; }
        public ushort Version { get; set; }

        public T FromStream(Stream s) {
            Version = s.BigEndianReadUShort();   // Field index 0 //
            DecodeRemainingFields(s);
            return (T)this;
        }

        public void ToStream(Stream s) {
            s.EncodeUShort(Version);              // Field index 0 //
            EncodeRemainingStateTo(s);
        }

        protected AbstractData(ushort version) => Version = version;

        protected abstract void DecodeRemainingFields(Stream s);

        protected abstract void EncodeRemainingStateTo(Stream s);
    }
}