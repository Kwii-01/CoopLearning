using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Tools {
    public class Timer {
        public float Time { get; private set; } = 0f;
        public float Value { get; private set; } = 0f;

        public Timer(float time) {
            this.Time = time;
        }

        public static implicit operator float(Timer timer) => timer.Value;

        public void Update(float deltaTime) {
            if (this.Value >= this.Time) {
                return;
            }
            this.Value += deltaTime;
        }

        public bool IsTimeUp() {
            return this.Value >= this.Time;
        }

        public void Reset() {
            this.Value = 0f;
        }
    }
}