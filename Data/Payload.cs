using System;
using System.IO;
using InterlockLedger.Tags;

namespace HiringTest
{
    public class Payload<T> : ILTagExplicit<T>, IVersion where T : AbstractData<T>, new()
    {
        public Payload(ulong tagId, Stream s) : base(tagId, s) {
        }

        public Payload(ulong tagId, T data) : base(tagId, data) {
        }

        public string TypeName => typeof(T).Name;

        public ushort Version {
            get => Value.Version;
            set => Value.Version = value;
        }

        public override string ToString() => Value.ToString();

        internal static Payload<T> DecodePayload(ulong tagId, Stream s)
            => s.DecodeTagId() == tagId ? TryBuildFrom(() => new Payload<T>(tagId, s)) : throw new InvalidDataException($"Not a Payload of {typeof(T).Name}");

        protected override T FromBytes(byte[] bytes)
            => FromBytesHelper(bytes, s => TryBuildFrom(() => new T().FromStream(s)));

        protected override byte[] ToBytes()
            => ToBytesHelper(s => Value.ToStream(s));

        private static TR TryBuildFrom<TR>(Func<TR> func) {
            try {
                return func();
            } catch (InvalidDataException e) {
                throw new InvalidDataException($"Not a properly encoded Payload of {typeof(T).Name}", e);
            }
        }
    }
}