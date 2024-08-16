using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Network.V2 {
    public interface IAction {
        public byte ID {
            get; set;
        }
        public void Launch(Vector3 position);
        public void Reconciliate(Vector3 position);
    }
}