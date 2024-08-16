using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;


using UnityEngine;
namespace Network.V1 {
    public class TransformState : INetworkSerializable {
        public int Tick;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsMoving;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            if (serializer.IsReader) {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Tick);
                reader.ReadValueSafe(out Position);
                reader.ReadValueSafe(out Rotation);
                reader.ReadValueSafe(out IsMoving);
            } else {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Tick);
                writer.WriteValueSafe(Position);
                writer.WriteValueSafe(Rotation);
                writer.WriteValueSafe(IsMoving);
            }
        }

    }
}