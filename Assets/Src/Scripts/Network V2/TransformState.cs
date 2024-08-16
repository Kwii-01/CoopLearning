using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;

namespace Network.V2 {
    public class TransformState : INetworkSerializable {
        public int Tick;
        public Vector3 Position;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref this.Tick);
            serializer.SerializeValue(ref this.Position);
        }
    }
}