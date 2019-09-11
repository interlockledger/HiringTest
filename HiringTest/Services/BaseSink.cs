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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using InterlockLedger.ILInt;
using InterlockLedger.Tags;
using InterlockLedger.Peer2Peer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace HiringTest
{
    internal abstract class BaseSink : AbstractNodeSink, IChannelSink
    {
        public BaseSink(string message, string nodeId) {
            PublishAtAddress = HostAtAddress = "localhost";
            PublishAtPortNumber = HostAtPortNumber = 8080;
            ListeningBufferSize = 512;
            DefaultTimeoutInMilliseconds = 30_000;
            MessageTag = 100;
            NetworkName = "HiringTest";
            NetworkProtocolName = "Hiring";
            NodeId = nodeId;
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _source = new CancellationTokenSource();
        }

        public override IEnumerable<string> LocalResources { get; } = new string[0];

        public override IEnumerable<string> SupportedNetworkProtocolFeatures { get; } = new string[] { "Who" };

        public static string AsString(IEnumerable<byte> text) => Encoding.UTF8.GetString(text.ToArray());

        public static byte[] AsUTF8Bytes(string s) => Encoding.UTF8.GetBytes(s);

        public override void HostedAt(string address, ushort port) {
            HostAtAddress = address;
            HostAtPortNumber = port;
        }

        public override void PublishedAt(string address, ushort port) {
            PublishAtAddress = address;
            PublishAtPortNumber = port;
        }

        public void Run() {
            PrepareConsole(_message);
            var serviceProvider = Configure(_source, portDelta: 4, this);
            using var peerServices = serviceProvider.GetRequiredService<IPeerServices>();
            Run(peerServices);
        }

        public byte[] ToMessage(ILTag payload) {
            var payloadBytes = payload.EncodedBytes;
            return MessageTag.AsILInt().Append(((ulong)payloadBytes.Length).AsILInt()).Append(payloadBytes);
        }

        protected readonly CancellationTokenSource _source;

        protected abstract void Run(IPeerServices peerServices);

        private readonly string _message;

        private static ServiceProvider Configure(CancellationTokenSource source, ushort portDelta, INetworkConfig config) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            return new ServiceCollection()
                .AddLogging(builder =>
                    builder
                        .AddConsole(c => c.DisableColors = false)
                        .SetMinimumLevel(LogLevel.Information))
                .AddSingleton(sp => new SocketFactory(sp.GetRequiredService<ILoggerFactory>(), portDelta))
                .AddSingleton<IExternalAccessDiscoverer, DummyExternalAccessDiscoverer>()
                .AddSingleton(sp =>
                    new PeerServices(
                        config.MessageTag, config.NetworkName, config.NetworkProtocolName, config.ListeningBufferSize,
                        sp.GetRequiredService<ILoggerFactory>(),
                        sp.GetRequiredService<IExternalAccessDiscoverer>(),
                        sp.GetRequiredService<SocketFactory>()).WithCancellationTokenSource(source))
                .BuildServiceProvider();
        }

        private CancellationTokenSource PrepareConsole(string message) {
            void Cancel(object sender, ConsoleCancelEventArgs e) {
                Console.WriteLine("Exiting...");
                _source.Cancel();
            }
            Console.WriteLine(message);
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += Cancel;
            return _source;
        }

        protected void Send(IActiveChannel channel, ILTag payload) {
            try {
                channel.Send(ToMessage(payload));
            } catch (SocketException) {
                // Do Nothing
            }
        }
    }
}