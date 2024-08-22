using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Chara {
    public interface IMovement {
        public float Speed {
            get;
            set;
        }


        public void Move(Vector3 direction, float deltaTime);
        public void Rotate(Vector3 direction, float deltaTime);
        public void Teleport(Vector3 position);
        public void Stop();

        public void OnUpdate(float deltaTime);
    }
}