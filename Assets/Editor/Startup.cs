using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class Startup {
    static Startup() {
#if UNITY_EDITOR && UNITY_STANDALONE_OSX
        UnityEditor.OSXStandalone.UserBuildSettings.architecture = UnityEditor.Build.OSArchitecture.x64;
#endif
    }
}