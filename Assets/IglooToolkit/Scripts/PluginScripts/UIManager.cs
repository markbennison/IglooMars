using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Igloo {
    [System.Serializable]

    public class UIManager : MonoBehaviour {

        private static UIManager instance;
        public static UIManager Instance {
            get {
                if (instance == null) instance = FindObjectOfType<UIManager>();
                return instance;
            }
        }

        public DrawUI drawUI;       
        public GameObject[] screens;
        public GameObject activeScreen;
        private bool uiSetupDone = false;
        private bool useUI = false;
        private string screenName;

        private Vector3 startScreenPos;
        private Vector3 startScreenRot;
        private Vector3 startScreenScale;

        public Canvas canvas;
        public RectTransform cursor;
        public RenderTexture texture;

        private PlayerManager.ROTATION_MODE? rotationMode = null;


        public void Setup(){
            if (canvas && cursor && texture && useUI) {
                SetScreen(screenName);
                drawUI.canvasUI = canvas;
                drawUI.cursorUI = cursor;
                activeScreen.transform.localPosition    = startScreenPos;
                activeScreen.transform.localEulerAngles = startScreenRot;
                activeScreen.transform.localScale       = startScreenScale;
                activeScreen.GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
                drawUI.Initialise();
                uiSetupDone = true;
            }
            else uiSetupDone = false;            
        }

        public void SetSettings (UISettings us) {
            if (us == null) {
                startScreenPos      = activeScreen.transform.localPosition;
                startScreenRot      = activeScreen.transform.localEulerAngles;
                startScreenScale    = activeScreen.transform.localScale;
            }
            useUI                   = us.useUI;
            screenName              = us.screenName;
            drawUI.followCrosshair  = us.followCrosshair;
            drawUI.movementSpeed    = us.followSpeed;
            startScreenPos          = us.screenPos.Vector3;
            startScreenRot          = us.screenRot.Vector3;
            startScreenScale        = us.screenScale.Vector3;                      
        }

        public UISettings GetSettings() {
            UISettings us   = new UISettings();
            if (activeScreen != null) {
                us.useUI            = useUI;
                us.screenName       = activeScreen.name;
                us.followCrosshair  = drawUI.followCrosshair;
                us.followSpeed      = drawUI.movementSpeed;
                us.screenPos        = new Vector3Item (startScreenPos);
                us.screenRot        = new Vector3Item (startScreenRot);
                us.screenScale      = new Vector3Item (startScreenScale);
            }
            return us;
        }

        public void SetScreen(string name) {
            bool found = false;
            for (int i = 0; i < screens.Length; i++) {
                if (screens[i].name == name) {
                    activeScreen = screens[i];
                    if (activeScreen.GetComponent<DrawUI>() != null) {
                        drawUI = activeScreen.GetComponent<DrawUI>();
                        found = true;
                    } 
                }
            }
            if (!found) Debug.LogError("Screen name " + name + " not found, using default screen instead");

        }

        public void SetUIVisible(bool state) {
            if (activeScreen && uiSetupDone) {
                canvas.enabled = state;
                activeScreen.SetActive(state);
                PlayerPointer.Instance.SetUIActive(state);

                if (state) {
                    if (IglooManager.Instance.GetPlayerManager()) {
                        rotationMode = IglooManager.Instance.GetPlayerManager().rotationMode;
                        IglooManager.Instance.GetPlayerManager().rotationMode = PlayerManager.ROTATION_MODE.IGLOO_360;
                    }
                }
                else if (!state) {
                    if (IglooManager.Instance.GetPlayerManager()) {
                        IglooManager.Instance.GetPlayerManager().rotationMode = (PlayerManager.ROTATION_MODE) rotationMode;
                    }
                }

            } 
        }

        public void SetFollowCursor(bool state) {
            drawUI.followCrosshair = state;
        }
        
    }
}

