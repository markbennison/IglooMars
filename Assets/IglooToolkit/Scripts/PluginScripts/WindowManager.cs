using System;
using System.Collections;
using UnityEngine;

namespace Igloo {
    public class WindowManager : MonoBehaviour {

        WindowSettings windowSettings = null;

        public void SetupWindows() {
            if (windowSettings == null) return;
            if (windowSettings.enabled == true) {
                Debug.Log("multi window is enabled, about to create windows");
                if (windowSettings.Windows != null) {
                    StartCoroutine("ApplyWindowSettings");
                }
            }
        }

        IEnumerator ApplyWindowSettings() {
            for (int i = 0; i < UnityEngine.Display.displays.Length; i++) {
                UnityEngine.Display.displays[i].Activate();
                if (i >= windowSettings.Windows.Length) { 
                    Debug.LogWarning("No settings found for display " + i + "creating default now");
                    WindowItem window = new WindowItem() { width = 0, height = 0, positionOffsetX = 0, positionOffsetY = 0 };
                    Array.Resize<WindowItem>(ref windowSettings.Windows, windowSettings.Windows.Length + 1);
                    windowSettings.Windows[windowSettings.Windows.Length - 1] = window;
                }
            }

            yield return new WaitForSeconds(1);

            for (int i = 0; i < UnityEngine.Display.displays.Length; i++) {
                if (i < windowSettings.Windows.Length) {
                    WindowItem window = windowSettings.Windows[i];
                    if (window.width > 0 && window.height > 0) 
                        UnityEngine.Display.displays[i].SetParams(window.width, window.height, window.positionOffsetX, window.positionOffsetY);
                }
                //yield return new WaitForSeconds(0.2f);
            }

        }
        public void SetSettings(WindowSettings s) {
            windowSettings = s;           
        }
        public WindowSettings GetSettings() {
            if (windowSettings == null) {
                // Create default window setting
                windowSettings = new WindowSettings();
                windowSettings.enabled = false;
                WindowItem[] windows = new WindowItem[1];
                WindowItem window = new WindowItem() { width = 0, height = 0, positionOffsetX = 0, positionOffsetY = 0 };
                windows[0] = window;
                windowSettings.Windows = windows;
            }
            return windowSettings;
        }
    }
}

