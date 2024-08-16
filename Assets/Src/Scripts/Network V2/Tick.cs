using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Network {
    public class Tick {
        public int Value { get; private set; } = 0;
        public int MaxValue { get; private set; } = 0;
        public int TickRatePerSecond { get; private set; } = 60;

        public float Delta { get; private set; } = 0f;
        private float _timer;

        public Tick(int tickRate = 60, int maxTick = 1024) {
            this.TickRatePerSecond = tickRate;
            this.MaxValue = maxTick;
            this.Delta = 1f / (float)tickRate;
        }

        public static implicit operator int(Tick tick) => tick.Value;

        public void Update(float deltaTime) {
            this._timer += deltaTime;
        }

        public bool CanTick() {
            if (this._timer >= this.Delta) {
                this._timer -= this.Delta;
                if (this.Value >= this.MaxValue) {
                    this.Value = 0;
                } else {
                    this.Value++;
                }
                return true;
            }
            return false;
        }

        public override string ToString() {
            return "Tick: " + this.Value + " MAX TICK: " + this.MaxValue + " FPS: " + this.TickRatePerSecond + " delta: " + this.Delta;
        }
    }
}