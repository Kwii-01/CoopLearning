using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Chara {
    public class Character : MonoBehaviour {
        public IMovement Movement;

        private void Awake() {
            this.Movement = this.GetComponent<IMovement>();
        }
    }
}