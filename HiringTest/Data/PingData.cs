/******************************************************************************************************************************

Copyright (c) 2018-2019 InterlockLedger Network
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

* Neither the name of the copyright holder nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

******************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InterlockLedger.Tags;

namespace HiringTest
{
    public class PingData : AbstractData<PingData>
    {
        public const int DefinedTagId = 128;

        public PingData(string id, DateTimeOffset now, params string[] features) : base(_currentVersion) {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Now = now;
            Features = new HashSet<string>(features);
        }

        public PingData() : base(_currentVersion) {
        }

        public HashSet<string> Features { get; set; }
        public string Id { get; set; }
        public DateTimeOffset Now { get; set; }
        public override ulong TagId => DefinedTagId;
        public bool Visible { get; set; }

        public override string ToString() => $"Client {Id}";

        protected override void DecodeRemainingFields(Stream s) {
            Id = s.DecodeString();
            Now = s.DecodeDateTimeOffset();
            Features = new HashSet<string>(s.DecodeTagArray<ILTagString>().Select(t => t.Value));
            Visible = Version > 1 ? s.DecodeBool() : false;
        }

        protected override void EncodeRemainingStateTo(Stream s) {
            s.EncodeString(Id);
            s.EncodeDateTimeOffset(Now);
            s.EncodeTagArray(Features?.Select(f => new ILTagString(f)));
            s.EncodeBool(Visible);
        }

        private const ushort _currentVersion = 2;
    }
}