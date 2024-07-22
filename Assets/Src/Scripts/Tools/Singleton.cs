using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;

namespace Tools {
    public class Singleton {
        private abstract class AWrapper {

        }

        private class Wrapper<T> : AWrapper {
            private T _storedValue;
            public T Value {
                get => this._storedValue;
                private set {
                }
            }

            public Wrapper(T storedValue) {
                this._storedValue = storedValue;
            }
        }

        private static Singleton _instance = null;
        private Dictionary<Type, AWrapper> _singletons = new Dictionary<Type, AWrapper>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() {
            _instance = new Singleton();
        }

        private Singleton() {
        }

        public static bool Set<T>(T singleton, bool ddol = false) {
            if (singleton is MonoBehaviour) {
                return _instance.m_SetMonoBehaviour(singleton, singleton as MonoBehaviour, typeof(T), ddol);
            }
            return _instance.m_Set(singleton, typeof(T));
        }

        public static T Get<T>() => _instance.m_Get<T>();


        private T m_Get<T>() {
            Type type = typeof(T);
            if (this._singletons.TryGetValue(type, out AWrapper singleton)) {
                return (singleton as Wrapper<T>).Value;
            }
#if UNITY_EDITOR
            Debug.LogError(type.ToString() + ": isn't a singleton yet or is not set in this provider.");
#endif
            return default;
        }

        private bool m_Set<T>(T singleton, Type type) {
            if (this._singletons.ContainsKey(type)) {
                return false;
            }
            this._singletons[type] = new Wrapper<T>(singleton);
            return true;
        }

        private bool m_SetMonoBehaviour<T, U>(T singleton, U casted, Type type, bool ddol) where U : MonoBehaviour {
            if (this.m_Set(singleton, type) == false) {
                GameObject.Destroy(casted.gameObject);
                return false;
            }
            if (ddol) {
                GameObject.DontDestroyOnLoad(casted.gameObject);
            }
            return true;
        }
    }
}