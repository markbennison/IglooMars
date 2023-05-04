using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Igloo {
    [ExecuteInEditMode]
    public class Display : MonoBehaviour
    {
        public string Name;
        public bool autoSetupOnStart = false;
        public HeadManager headManager;
        public DisplayManager displayManager;
        public GameObject cameraPrefab;

        public enum IglooCubemapFace {Left,Front,Right,Back,Down,Up}
        public IglooCubemapFace iglooCubemapFace;
        //public int cubemapFace;

        [SerializeField]
        protected  Dictionary<Igloo.EYE, Camera> activeCameras;
        public  Dictionary<Igloo.EYE, Camera> GetActiveCameras() {return activeCameras;}

        /// <summary>
        /// Returns a list of the Camera Components. 
        /// If Display is 3D there will be 2 cameras; left, right
        /// If Display is not 3D there will be 1 camera; center
        /// </summary>
        /// <returns></returns>
        public List<Camera> GetCameras() {
            List<Camera> cams = new List<Camera>();
            foreach (var cam in activeCameras) {
                cams.Add(cam.Value);
            }
            return cams;
        }

        public RenderTexture leftTexture = null;
        public RenderTexture centerTexture = null;
        public RenderTexture rightTexture = null;

        protected float fov;
        public virtual float FOV {
            get { return fov; }
            set { foreach (var cam in activeCameras) {
                    cam.Value.fieldOfView = value;
                }
                fov = value;
            }
        }

        protected float nearClipPlane = 0.01f;
        public float NearClipPlane {
            get { return nearClipPlane; }
            set {
                foreach (var cam in activeCameras) {
                    cam.Value.nearClipPlane = value;
                }
                nearClipPlane = value;
            }
        }

        protected float farClipPlane = 1000.0f;
        public float FarClipPlane {
            get { return farClipPlane; }
            set {
                foreach (var cam in activeCameras) {
                    cam.Value.farClipPlane = value;
                }
                farClipPlane = value;
            }
        }

        protected Vector3 camRotation;

        // Value set from settings file and should not be modified during runtime 
        private bool isRendering;

        public bool SetRendering {
            get { return isRendering; }
            set {
                if (isRendering) {
                    foreach (var cam in activeCameras) {
                        cam.Value.enabled = value;
                    }
                }                   
            }
        }

        protected bool is3D;
        public bool Is3D { get { return is3D; } }

        protected bool isRenderTextures;
        public bool IsRenderTextures {get{ return IsRenderTextures; } }

        protected Vector2Int renderTextureSize;
        public Vector2Int RenderTextureSize { get { return renderTextureSize; } }

        protected Rect viewPortRect;
        public Rect ViewportRect {
            get { return viewPortRect; }
            set {
                foreach (var cam in activeCameras) {
                    cam.Value.rect = value;
                }
                viewPortRect = value;
            }
        }
        protected bool isFisheye;
        public bool IsFisheye {
            get { return isFisheye; }
            set {
                foreach (var cam in activeCameras) {
                    if (value) {
                        if (cam.Value.gameObject.GetComponent<Fisheye>() == null) cam.Value.gameObject.AddComponent<Fisheye>();
                    }
                    else if (!value) {
                        if (cam.Value.gameObject.GetComponent<Fisheye>()) Destroy(gameObject.GetComponent<Fisheye>());
                    }
                }
                isFisheye = value;
            }
        }
        protected Vector2 fisheyeStrength;
        public Vector2 FisheyeStrength {
            get { return fisheyeStrength; }
            set {
                foreach (var cam in activeCameras) {
                    if (cam.Value.gameObject.GetComponent<Fisheye>() != null) {
                        cam.Value.gameObject.GetComponent<Fisheye>().strengthX = value.x;
                        cam.Value.gameObject.GetComponent<Fisheye>().strengthX = value.x;
                    }
                }
                fisheyeStrength = value;
            }
        }

        public bool isOffAxis;

        public float viewportWidth;
        public float viewportHeight;

        protected float halfWidth() { return viewportWidth * 0.5f; }
        protected float halfHeight() { return viewportHeight * 0.5f; }

        public Vector3 UpperRight   { get { return transform.localToWorldMatrix * new Vector4(halfWidth(), halfHeight(), 0.0f, 1.0f); } }
        public Vector3 UpperLeft    { get { return transform.localToWorldMatrix * new Vector4(-halfWidth(), halfHeight(), 0.0f, 1.0f); } }       
        public Vector3 LowerLeft    { get { return transform.localToWorldMatrix * new Vector4(-halfWidth(), -halfHeight(), 0.0f, 1.0f); } }       
        public Vector3 LowerRight   { get { return transform.localToWorldMatrix * new Vector4(halfWidth(), -halfHeight(), 0.0f, 1.0f); } }

        public virtual void Awake() {
            if (Application.isPlaying && transform.parent == null && IglooManager.Instance.dontDestroyOnLoad) DontDestroyOnLoad(this.gameObject);
        }

        public virtual void LateUpdate() {
            // update camera projection matrices for offaxis projection
            if (isOffAxis) {
                // Old Method
                foreach (var cam in activeCameras) {
                    cam.Value.projectionMatrix = Utils.getAsymProjMatrix(LowerLeft, LowerRight, UpperLeft, cam.Value.gameObject.transform.position, headManager.NearClipPlane, headManager.FarClipPlane);
                    cam.Value.gameObject.transform.rotation = transform.rotation;
                }

                // Stereo Matrix Test
                //bool isSetStereoMat = false;
                //foreach (var cam in activeCameras)
                //{
                //    if (cam.Key == EYE.CENTER) {
                //        cam.Value.projectionMatrix = Utils.getAsymProjMatrix(LowerLeft, LowerRight, UpperLeft, cam.Value.gameObject.transform.position, headManager.NearClipPlane, headManager.FarClipPlane);
                //    }
                //    else if (cam.Key == EYE.LEFT)  {
                //        if (isSetStereoMat) {
                //            Matrix4x4 leftMatrix = Utils.getAsymProjMatrix(LowerLeft, LowerRight, UpperLeft, cam.Value.gameObject.transform.position, headManager.NearClipPlane, headManager.FarClipPlane);
                //            cam.Value.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, leftMatrix);
                //        }
                //        else cam.Value.projectionMatrix = Utils.getAsymProjMatrix(LowerLeft, LowerRight, UpperLeft, cam.Value.gameObject.transform.position, headManager.NearClipPlane, headManager.FarClipPlane);
                //    }
                //    else if (cam.Key == EYE.RIGHT)
                //    {
                //        if (isSetStereoMat) {
                //            Matrix4x4 rightMatrix = Utils.getAsymProjMatrix(LowerLeft, LowerRight, UpperLeft, cam.Value.gameObject.transform.position, headManager.NearClipPlane, headManager.FarClipPlane);
                //            cam.Value.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, rightMatrix);
                //        }
                //        else cam.Value.projectionMatrix = Utils.getAsymProjMatrix(LowerLeft, LowerRight, UpperLeft, cam.Value.gameObject.transform.position, headManager.NearClipPlane, headManager.FarClipPlane);
                //    }
                //    cam.Value.gameObject.transform.rotation = transform.rotation;
                //}
            }
        }

        public virtual void SetSettings(DisplayItem settings) {
            Name        = settings.Name;
            isRendering = settings.isRendering;
            is3D        = settings.is3D;
            isOffAxis   = settings.isOffAxis;
            fov         = settings.fov;
            isFisheye   = settings.isFisheye;
            isRenderTextures    = settings.isRenderTextures;
            iglooCubemapFace    = (IglooCubemapFace)settings.cubemapFace;

            if (settings.nearClipPlane != 0) nearClipPlane                          = settings.nearClipPlane;
            if (settings.farClipPlane != 0) farClipPlane                            = settings.farClipPlane;
            if (settings.cameraRotation != null) camRotation                        = settings.cameraRotation.Vector3;
            if (settings.viewportRotation != null) this.transform.localEulerAngles  = settings.viewportRotation.Vector3;
            if (settings.renderTextureSize != null) renderTextureSize               = settings.renderTextureSize.Vector2Int;
            if (settings.viewportPosition != null) this.transform.localPosition     = settings.viewportPosition.Vector3;
            if (settings.viewportSize != null) {
                viewportWidth   = settings.viewportSize.x;
                viewportHeight  = settings.viewportSize.y;
            }
            if (settings.fisheyeStrength != null) {
                fisheyeStrength.x = settings.fisheyeStrength.x;
                fisheyeStrength.y = settings.fisheyeStrength.y;
            }

        }

        public virtual DisplayItem GetSettings() {
            DisplayItem settings = new DisplayItem();
            settings.Name = Name;
            settings.isRendering = isRendering;
            settings.cameraRotation = new Vector3Item(camRotation); 
            settings.is3D = is3D;
            settings.isOffAxis = isOffAxis;
            settings.viewportPosition = new Vector3Item(this.transform.localPosition);
            settings.viewportSize = new Vector2Item(viewportWidth,viewportHeight);
            settings.fov = fov;
            settings.isFisheye = isFisheye;
            settings.isRenderTextures = isRenderTextures;
            settings.cubemapFace = (int)iglooCubemapFace;
            settings.viewportRotation = new Vector3Item(this.transform.localEulerAngles);
            settings.nearClipPlane = nearClipPlane;
            settings.farClipPlane = farClipPlane;
            settings.fisheyeStrength = new Vector2Item(fisheyeStrength);
            if (renderTextureSize != null) settings.renderTextureSize = new Vector2IntItem(renderTextureSize);
           
            return settings;
        }

        public virtual void InitialiseCameras() {}

        public virtual void SetupDisplay() {
            activeCameras = new Dictionary<Igloo.EYE, Camera>();

            // Creates Camera objects and adds them to the activeCameras dictionary
            InitialiseCameras();

            IsFisheye = isFisheye;
            FisheyeStrength = fisheyeStrength;

            foreach (var cam in activeCameras) {
                if (!isOffAxis) cam.Value.fieldOfView = fov;               
                cam.Value.enabled = isRendering;
                cam.Value.nearClipPlane = nearClipPlane;
                cam.Value.farClipPlane = farClipPlane;

#if IGLOO_URP
                // For URP - postprocessing is off by default
                var cameraData = cam.Value.GetUniversalAdditionalCameraData();
                cameraData.renderPostProcessing = true;
#endif
                if (cam.Key == EYE.LEFT) {
                    if (isRenderTextures) {
                        leftTexture = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 0);
                        leftTexture.name = gameObject.name + "_" + cam.Key;
                        cam.Value.targetTexture = leftTexture;
                    }
                }
                else if (cam.Key == EYE.CENTER){
                    if (isRenderTextures) {
                        centerTexture = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 0);
                        centerTexture.name = gameObject.name + "_" + cam.Key;
                        cam.Value.targetTexture = centerTexture;
                    }
                }
                else if (cam.Key == EYE.RIGHT) {
                    if (isRenderTextures) {
                        rightTexture = new RenderTexture(renderTextureSize.x, renderTextureSize.y, 0);
                        rightTexture.name = gameObject.name + "_" + cam.Key;
                        cam.Value.targetTexture = rightTexture;
                    }
                }
            }

            headManager.OnHeadSettingsChange += HeadSettingsChanges;
        }

        void HeadSettingsChanges() {
            foreach (var cam in activeCameras) {
                headManager.ApplyCameraSettings(cam.Value, cam.Key);
            }
        }

        void EditorDraw() {
            var mat = transform.localToWorldMatrix;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(UpperRight, UpperLeft);
            Gizmos.DrawLine(UpperLeft, LowerLeft);
            Gizmos.DrawLine(LowerLeft, LowerRight);
            Gizmos.DrawLine(LowerRight, UpperRight);
        }

        void OnDrawGizmos() {
            if (isOffAxis)EditorDraw();
        }

        private void OnDrawGizmosSelected() {
            if (isOffAxis) {
                var mat = transform.localToWorldMatrix;
                Vector3 right = mat * new Vector4(halfWidth() * 0.75f, 0.0f, 0.0f, 1.0f);
                Vector3 up = mat * new Vector4(0.0f, halfHeight() * 0.75f, 0.0f, 1.0f);
                Gizmos.color = new Color(0.75f, 0.25f, 0.25f);
                Gizmos.DrawLine((transform.position * 2.0f + right) / 3.0f, right);
                Gizmos.color = new Color(0.25f, 0.75f, 0.25f);
                Gizmos.DrawLine((transform.position * 2.0f + up) / 3.0f, up);
            }
        }

        public virtual void OnDestroy() {
            if (activeCameras != null) {
                foreach (var cam in activeCameras) {
                    if (cam.Value != null) DestroyImmediate(cam.Value.gameObject);
                }
            }

        }
    }
}

