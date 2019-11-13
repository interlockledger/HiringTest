using System;
using System.Collections.Generic;
using System.IO;
using InterlockLedger.Tags;

namespace HiringTest
{
    public class TagDeserializersProvider : ITagDeserializersProvider
    {
        public static readonly TagDeserializersProvider Instance = new TagDeserializersProvider();

        public IEnumerable<(ulong id, Func<Stream, ILTag> deserializer, Func<object, ILTag> jsonDeserializer)> Deserializers {
            get {
                yield return (PingData.DefinedTagId, s => new Payload<PingData>(PingData.DefinedTagId, s), ILTag.NoJson);
                yield return (PongData.DefinedTagId, s => new Payload<PongData>(PongData.DefinedTagId, s), ILTag.NoJson);
                ;
            }
        }

        private TagDeserializersProvider() { }
    }
}