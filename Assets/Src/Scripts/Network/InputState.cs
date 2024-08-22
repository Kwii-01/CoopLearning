using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using Unity.Netcode;

using UnityEngine;

namespace Network {
    public class InputState : INetworkSerializable {
        public int Tick;
        public short MoveX;
        public short MoveY;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref this.Tick);
            serializer.SerializeValue(ref this.MoveX);
            serializer.SerializeValue(ref this.MoveY);
        }
    }

}
public static class Quantize {
    public static short QuantizeFloat(this float value, float min, float max) {
        return (short)((value - min) / (max - min) * short.MaxValue);
    }

    public static float DequantizeShort(this short value, float min, float max) {
        return min + (value / (float)short.MaxValue) * (max - min);
    }
}