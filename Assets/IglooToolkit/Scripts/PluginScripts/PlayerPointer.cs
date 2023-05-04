using System.Collections;
using System.Collections.Generic;
//using Tayx.Graphy.Utils.NumString;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Igloo {
    public class PlayerPointer : MonoBehaviour
    {
        bool isInit = false;

        bool draw3D = false;
        public bool GetDraw3D() {return draw3D;}
        
        bool uiActive = false;

        public float size = 0.015f;

        public GameObject crosshair;
        Renderer crosshairRenderer;
        Vector3 initPos;
        Vector3 initRot;
        Vector3 initScal;

        private static PlayerPointer instance;
        public static PlayerPointer Instance {
            get {
                if (instance == null) instance = FindObjectOfType<PlayerPointer>();
                return instance;
            }
        }

        bool hasHit = false;

        private void Awake() {
            if (!isInit) Init();          
        }

        private void Init() {
            crosshairRenderer = crosshair.GetComponent<Renderer>();
            initPos     = crosshair.transform.localPosition;
            initRot     = crosshair.transform.localEulerAngles;
            initScal    = crosshair.transform.localScale;
            isInit = true;
        }

        public void SetDraw3D(bool state) {
            if (!isInit) Init();
            if (state) {
                if (crosshair) {
                    initPos     = crosshair.transform.localPosition;
                    initRot     = crosshair.transform.localEulerAngles;
                    initScal    = crosshair.transform.localScale;
                }
            }
            else if (!state) {
                if (crosshair) {
                    crosshair.transform.localPosition       = initPos;
                    crosshair.transform.localScale          = initScal;
                    crosshair.transform.localEulerAngles    = initRot;
                }
                
            }
            draw3D = state;
        }

        public void SetUIActive(bool state) {
            if (!isInit) Init();

            if (state && !draw3D) {
                crosshair.transform.localScale = new Vector3(size, size, size);
            }
            if (!state && !draw3D) {
                    if (crosshair) {
                        crosshair.transform.localPosition       = initPos;
                        crosshair.transform.localScale          = initScal;
                        crosshair.transform.localEulerAngles    = initRot;
                    }
            }
            uiActive = state;
        }

        private void Update() {
            if (!draw3D && !uiActive) return;

            int layerMask = ~0;
            if (uiActive == true) layerMask = LayerMask.GetMask("IglooScreen");
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask)) {
                crosshair.transform.position = hit.point;

                // Detect screen collision
                if (hit.transform.gameObject.tag == "IglooScreen"){
                    ScreenHit(hit.textureCoord);
                }
                else
                    ScreenMiss();
                
                // Crosshair positioning system
                crosshair.transform.rotation = Quaternion.FromToRotation(crosshair.transform.up, hit.normal) * crosshair.transform.rotation;
                if (!hasHit) {
                    crosshair.transform.localScale = new Vector3(size, size, size);
                    hasHit = true;
                }
            }
            else {
                ScreenMiss();
                // Crosshair positioning system
                crosshair.transform.localPosition       = initPos;
                crosshair.transform.localScale          = initScal;
                crosshair.transform.localEulerAngles    = initRot;
                hasHit = false;
            }
            
        }

        public ScreenHitPosition OnScreenHitPosition;
        public delegate void ScreenHitPosition(Vector2 pos);

        public ScreenMissCallback OnScreenMiss;
        public delegate void ScreenMissCallback();
        private void ScreenHit(Vector2 pos) {
            if (OnScreenHitPosition != null) OnScreenHitPosition(pos);
        }
        private void ScreenMiss(){
            if (OnScreenHitPosition != null) OnScreenMiss();
        }
    }
}
    
