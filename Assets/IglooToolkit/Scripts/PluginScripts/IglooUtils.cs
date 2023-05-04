using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;

namespace Igloo {

    // Enums
    public enum EYE { LEFT, CENTER, RIGHT }
    public enum VSyncMode {DONT,EVERY,EVERY_SECOND}

    public static class Utils
    {
        public static string GetVersion() {
            return "1.1.0";
        }

        public static bool IsAdvancedMode() {
            return false;
        }

        public static Matrix4x4 getAsymProjMatrix(Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pe, float ncp, float fcp) {
            //compute orthonormal basis for the screen - could pre-compute this...
            Vector3 vr = (pb - pa).normalized;
            Vector3 vu = (pc - pa).normalized;
            Vector3 vn = Vector3.Cross(vr, vu).normalized;

            //compute screen corner vectors
            Vector3 va = pa - pe;
            Vector3 vb = pb - pe;
            Vector3 vc = pc - pe;

            //find the distance from the eye to screen plane
            float n = ncp;
            float f = fcp;
            float d = Vector3.Dot(va, vn); // distance from eye to screen
            float nod = n / d;
            float l = Vector3.Dot(vr, va) * nod;
            float r = Vector3.Dot(vr, vb) * nod;
            float b = Vector3.Dot(vu, va) * nod;
            float t = Vector3.Dot(vu, vc) * nod;

            //put together the matrix - bout time amirite?
            Matrix4x4 m = Matrix4x4.zero;

            //from http://forum.unity3d.com/threads/using-projection-matrix-to-create-holographic-effect.291123/
            m[0, 0] = 2.0f * n / (r - l);
            m[0, 2] = (r + l) / (r - l);
            m[1, 1] = 2.0f * n / (t - b);
            m[1, 2] = (t + b) / (t - b);
            m[2, 2] = -(f + n) / (f - n);
            m[2, 3] = (-2.0f * f * n) / (f - n);
            m[3, 2] = -1.0f;

            return m;
        }

        public static string StringSplitter(string s, char[] delim, int at) {
            string _s = null;
            string[] split = s.Split(delim);
            _s = split[at];
            return _s;
        }

        public static void CopySpecialComponents(GameObject sourceGO, GameObject targetGO) {
#if UNITY_EDITOR
            foreach (var component in sourceGO.GetComponents<Component>()) {
                var componentType = component.GetType();
                if (componentType != typeof(Transform) &&
                    componentType != typeof(MeshFilter) &&
                    componentType != typeof(MeshRenderer) &&
                    componentType != typeof(Camera) &&
                    componentType != typeof(AudioListener) &&
                    componentType != typeof(AudioListener)
                    )
                {
                    Debug.Log("Found a component of type " + component.GetType());
                    UnityEditorInternal.ComponentUtility.CopyComponent(component);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetGO);
                    Debug.Log("Copied " + component.GetType() + " from " + sourceGO.name + " to " + targetGO.name);
                }
            }
#endif
        }

        public static void setWindowPostion(int x, int y) {
     #if UNITY_STANDALONE_WIN || UNITY_EDITOR
        SetWindowPos(FindWindow(null, Application.productName), 0, x, y, Screen.width, Screen.height, Screen.width * Screen.height == 0 ? 1 : 0);   
#endif
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(System.String className, System.String windowName);
#endif

        public static string GetWarperDataPath() {
            string appdata = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appdata, "Igloo Vision\\IglooWarper");
        }

    }
}

