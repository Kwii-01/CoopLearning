using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace Network {
    public class CircularBuffer<T> {
        private T[] _buffer;

        public CircularBuffer(int size) {
            this._buffer = new T[size];
        }

        public CircularBuffer(T[] buffer) {
            this._buffer = buffer;
        }

        public T this[int i] {
            get => this._buffer[i % this._buffer.Length];
            set => this._buffer[i % this._buffer.Length] = value;
        }

        public int Length => this._buffer.Length;

        public static implicit operator T[](CircularBuffer<T> c) => c._buffer;
        public static explicit operator CircularBuffer<T>(T[] buffer) => new CircularBuffer<T>(buffer);

        public override string ToString() {
            StringBuilder stringBuilder = new StringBuilder();
            int index = 0;
            foreach (var item in this._buffer) {
                if (item != null) {
                    stringBuilder.Append(index + ": ");
                    stringBuilder.Append(item.ToString());
                }
                index++;
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }

    }
}