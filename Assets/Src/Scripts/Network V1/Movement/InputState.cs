using System.Collections;
using System.Collections.Generic;

using UnityEngine;
namespace Network.V1 {
    public class InputState {
        public int Tick;
        public Vector2 MovementInput;
        public Vector2 LookInput;
        public bool Jump;
    }
}