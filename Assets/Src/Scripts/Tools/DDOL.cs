using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Tools {
    [DefaultExecutionOrder(-20)]
    public class DDOL : MonoBehaviour {
        private void Awake() {
            DontDestroyOnLoad(this);
        }
    }
}