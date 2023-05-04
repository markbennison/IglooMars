using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Igloo {

    public class Crosshair : MonoBehaviour
    {
        public enum CROSSHAIR_MODE {SHOW, SHOW_ON_MOVE, HIDE };
        public CROSSHAIR_MODE crosshairMode = CROSSHAIR_MODE.SHOW;

        Vector3 previousPos = new Vector3();
        private Renderer crosshairRenderer;

        bool forceHide = false;

        private void Awake() {
            crosshairRenderer = GetComponent<Renderer>();
        }

        private void Update(){
            forceHide = false;
        }

        void LateUpdate() {
            if (forceHide) {
                crosshairRenderer.enabled = false;
                return;
            }

            switch (crosshairMode){
                case CROSSHAIR_MODE.SHOW:
                    crosshairRenderer.enabled = true;
                    break;
                case CROSSHAIR_MODE.HIDE:
                    crosshairRenderer.enabled = false;
                    break;
                case CROSSHAIR_MODE.SHOW_ON_MOVE:
                    if (crosshairRenderer.enabled == false && transform.position != previousPos) {
                        crosshairRenderer.enabled = true;
                    }
                    else if (crosshairRenderer.enabled == true && transform.position == previousPos) {
                        crosshairRenderer.enabled = false;
                    }
                    previousPos = transform.position; break;
            }
        }

        public void SetMode(CROSSHAIR_MODE mode) {
            crosshairMode = mode;
        }

        // If the cursor is 
        public void ForceHide(bool state) {
            forceHide = state;
        }
    }
}
