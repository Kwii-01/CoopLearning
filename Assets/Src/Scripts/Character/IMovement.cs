using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Chara {
    public interface IMovement {
        public float Speed {
            get;
            set;
        }

        public void SetDeltaTime(float deltaTime);
        public void Move(Vector3 direction);
        public void Rotate(Vector3 direction);
        public void Teleport(Vector3 position);
        public void Stop();
    }
}