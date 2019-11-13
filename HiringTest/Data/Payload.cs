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