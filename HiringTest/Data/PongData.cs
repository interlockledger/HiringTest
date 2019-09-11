using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InterlockLedger.Tags;

namespace HiringTest
{
    public class PongData : AbstractData<PongData>
    {
        public const int DefinedTagId = 129;

        public PongData(string id, DateTimeOffset now, params string[] features) : base(_currentVersion) {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Now = now;
            Features = new HashSet<string>(features);
        }

        public PongData() : base(_currentVersion) {
        }

        public HashSet<string> Features { get; set; }
        public string Id { get; set; }
        public DateTimeOffset Now { get; set; }
        public override ulong TagId => DefinedTagId;

        public override string ToString() => $"Server {Id} with features {Features.JoinedBy(",")}";

        protected override void DecodeRemainingFields(Stream s) {
            Id = s.DecodeString();
            Now = s.DecodeDateTimeOffset();
            Features = new HashSet<string>(s.DecodeTagArray<ILTagString>().Select(t => t.Value));
        }

        protected override void EncodeRemainingStateTo(Stream s) {
            s.EncodeString(Id);
            s.EncodeDateTimeOffset(Now);
            s.EncodeTagArray(Features?.Select(f => new ILTagString(f)));
        }

        private const ushort _currentVersion = 1;
    }
}