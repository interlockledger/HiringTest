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
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InterlockLedger.Peer2Peer;
using InterlockLedger.Tags;

namespace HiringTest
{
    internal class Server : BaseSink
    {
        public Server() : base("Server", "Local Node") {
        }

        public string Url => $"{NetworkProtocolName.ToLowerInvariant()}://{PublishAtAddress}:{PublishAtPortNumber}/";

        public override Task<Success> SinkAsync(IEnumerable<byte> message, IActiveChannel channel) {
            _queue.Enqueue((message, channel));
            return Task.FromResult(Success.Next);
        }

        protected override void Run(IPeerServices peerServices) {
            using var listener = peerServices.CreateListenerFor(this);
            try {
                listener.Start();
                Dequeue().RunOnThread("Server");
                while (listener.Alive) {
                    Thread.Sleep(1);
                }
            } finally {
                _stop = true;
            }
        }

        private readonly ConcurrentQueue<(IEnumerable<byte> message, IActiveChannel activeChannel)> _queue = new ConcurrentQueue<(IEnumerable<byte> message, IActiveChannel activeChannel)>();

        private bool _stop = false;

        private async Task Dequeue() {
            do {
                if (_queue.TryDequeue(out var tuple))
                    await SinkAsServer(tuple.message, tuple.activeChannel);
                await Task.Delay(10);
            } while (!_stop);
        }

        private Task SinkAsServer(IEnumerable<byte> message, IActiveChannel channel) {
            if (message.Any()) {
                if (message.First() == PingData.DefinedTagId) {
                    var ping = (ILTag.DeserializeFrom(message.ToArray()) as Payload<PingData>).Value;
                    if (ping.Visible)
                        Console.WriteLine($@"[{channel}] Pinged from '{ping}'");
                    var pong = new PongData(Id, DateTimeOffset.Now, SupportedNetworkProtocolFeatures.ToArray());
                    Send(channel, pong.AsPayload);
                }
            }
            return Task.CompletedTask;
        }
    }
}