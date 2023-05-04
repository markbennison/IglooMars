using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Igloo
{
    [ExecuteInEditMode]
    public class DrawUI : MonoBehaviour
    {
        private float x = 0.0f;
        private float y = 0.0f;
        public float size = 1.0f;
        public float lastX = 0.0f;
        public bool followCrosshair = true;
        public bool hideCrosshairOnScreen = false;
        public Canvas canvasUI;
        public RectTransform cursorUI;
        Material mat;
        public float aspectRatioMesh = 3.5f;
        private float aspectRatioUI = 1.76f;
        private float targetX;
        public float movementSpeed = 10f;
        Crosshair crosshair;
        public float UVXHIT = 0.0f;
        bool initOnStart = false;
        bool animateUi = false;
        public bool on = true;
        [Tooltip("The speed multiplier for when moving the UI larger distances")]
        public float distSpeedMultiplier = 5.0f;
        [Tooltip("The minimum distance of UI travel where the speed multiplier kicks in")]
        public float distThreshold = 0.7f;
        public void setX(float newX) { x = newX; }
        public void setY(float newY) { y = newY; }

        bool isInit = false;

        public void Initialise() {
            PlayerPointer.Instance.OnScreenHitPosition += SetCursorPos;
            PlayerPointer.Instance.OnScreenMiss += SetCursorMiss; mat = GetComponent<Material>(); 
            crosshair       = Igloo.IglooManager.Instance.igloo.GetComponent<PlayerManager>().crosshair;
            aspectRatioUI   = canvasUI.pixelRect.width / canvasUI.pixelRect.height;
            aspectRatioMesh = GetMeshApectRatio();
            isInit          = true;
        }

        void Start() {
            if (initOnStart) Initialise();
        }

        float GetMeshApectRatio () {
            float w = gameObject.transform.localScale.x;
            float h = gameObject.transform.localScale.y;
            if (gameObject.name == "Cylinder"){
                return (2 * Mathf.PI * w / 2) / h;
            }
            else if (gameObject.name == "Plane") {
                return w / h;
            }
            else return 1;
        }

        void LateUpdate()
        {
            //Profiler.BeginSample("UI_Update");
            if (!on || !isInit) return;
            if (!mat) mat = GetComponent<Renderer>().material;

            float xScaleFactor = aspectRatioMesh / aspectRatioUI;

            float xScale = (1 + (1 - size)) * xScaleFactor;
            float yScale = 1 + (1 - size);
            mat.mainTextureScale = new Vector2(xScale, yScale);

            float xPos = -x * xScale;
            float yPos = -y;

            if (followCrosshair) {
                if (animateUi) {
                    if (lastX < 0.65f && lastX > 0.45f)
                        animateUi = false;
                    else {
                        if (lastX > 0.5f && lastX < (xScale*0.7f)- xPos )
                            gameObject.transform.Rotate(new Vector3(0.0f, movementSpeed, 0.0f));
                        else 
                            gameObject.transform.Rotate(new Vector3(0.0f, -movementSpeed, 0.0f));     
                    }
                }
            }
            mat.mainTextureOffset = new Vector2(xPos, yPos);
            //Profiler.EndSample();
        }
        
        public void SetCursorMiss() {
            animateUi = false;
            return;
        }

        public void SetCursorPos(Vector2 pos) {
            if (!mat) return;
            //Profiler.BeginSample("setCursor");
            Vector2 posMapped = new Vector2();
            posMapped.x = (pos.x * mat.mainTextureScale.x) + mat.mainTextureOffset.x;
            posMapped.y = (pos.y * mat.mainTextureScale.y) + mat.mainTextureOffset.y;
        
            lastX = posMapped.x;
            if (lastX > 1.0f || lastX <  0 )
                animateUi = true;

            if ((Mathf.Clamp(posMapped.x, 0, 1) == posMapped.x) && (Mathf.Clamp(posMapped.y, 0, 1) == posMapped.y)) { 
                if (hideCrosshairOnScreen && crosshair) crosshair.ForceHide(true);
                cursorUI.anchoredPosition = new Vector2(canvasUI.pixelRect.width * posMapped.x, canvasUI.pixelRect.height * posMapped.y);
            }
            
        }
    }
}
