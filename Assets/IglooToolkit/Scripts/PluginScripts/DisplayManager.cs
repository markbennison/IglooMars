using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Igloo {
    /// <summary>
    /// Creates and manages Display objects also allowing the following optional functionality 
    /// Compositing - multiple camera render textures can be combined into a single texture
    /// Cubemap conversion - resulting textures can be processed to perform cubemap-equirectangular conversion
    /// Sharing - resulting textures can be shared via the TextureShareUtility
    /// </summary>
    [ExecuteInEditMode]
    public class DisplayManager : MonoBehaviour
    {
        public HeadManager headManager;
        public GameObject cameraPrefab;
       
        [SerializeField]
        public List<Display> displays;
        public string sharingName = "IglooUnity";

        private RenderTexture outputTextureLeft   = null;
        private RenderTexture outputTextureCenter = null;
        private RenderTexture outputTextureRight  = null;

        public RenderTexture topBottom3DTexture = null;

        public bool useCompositeTexture     = false;
        public bool useFramepackTopBottom3D = false;
        private Material topBottomMaterial  = null;

        public TextureShareUtility.TextureShareMode textureShareMode = TextureShareUtility.TextureShareMode.NONE;

        // Warp & Blend 
        private bool            useWarpBlend = false;
        private string          warperDataPath;
        private int             targetDisplayStartIndex = 1;
        public WarpBlendManager warpBlendManager;

        #region Cubemap To Equirectangular

        public bool useCubemapToEqui = false;
        private Vector2Int equirectangularTexuteRes;
        private Material cubeToEquiMatLeft      = null;
        private Material cubeToEquiMatCenter    = null;
        private Material cubeToEquiMatRight     = null;
        private float horizontalFOV = 360f;
        private float verticalFOV = 70f;
        #endregion

        /// <summary>
        /// Setup Display manager and create Display object using settings 
        /// </summary>
        /// <param name="ds"></param>
        public void Setup(DisplaySettings ds) {
            if (ds.Name != null) sharingName = ds.Name;
            useCompositeTexture     = ds.useCompositeTexture;
            useCubemapToEqui        = ds.useCubemapToEquirectangular;
            horizontalFOV           = ds.horizontalFOV;
            verticalFOV             = ds.verticalFOV;
            useFramepackTopBottom3D = ds.useFramepackTopBottom3D;
            useWarpBlend            = ds.useWarpBlend;
            textureShareMode        = (TextureShareUtility.TextureShareMode)ds.textureShareMode;
            if(ds.equirectangularTexuteRes != null) equirectangularTexuteRes = ds.equirectangularTexuteRes.Vector2Int;

            if (headManager == null) {
                GameObject go = new GameObject("Head");
                if (this.gameObject.transform.parent != null) go.transform.parent = this.gameObject.transform;
                headManager = go.AddComponent<HeadManager>();
            }

            if(ds.HeadSettings != null) headManager.SetSettings(ds.HeadSettings);

            if (ds.useWarpBlend) {
                if (!GetComponent<WarpBlendManager>()) gameObject.AddComponent<WarpBlendManager>();
                warpBlendManager = GetComponent<WarpBlendManager>();
                warpBlendManager.SetSettings(ds.WarpBlendSettings);
            }


            CreateDisplays(ds.Displays);
            if (useCubemapToEqui) SetupCubemapToEqui();
            else if (useCompositeTexture) SetupComposition();
            SetupTextureSharing();
        }

        /// <summary>
        /// Returns DisplaySettings, used by serialiser for saving
        /// </summary>
        /// <returns></returns>
        public DisplaySettings GetSettings() {
            DisplaySettings settings = new DisplaySettings();
            settings.Name                       = sharingName;
            settings.useCompositeTexture        = useCompositeTexture;
            settings.useCubemapToEquirectangular= useCubemapToEqui;
            settings.horizontalFOV              = horizontalFOV;
            settings.verticalFOV                = verticalFOV;
            settings.useFramepackTopBottom3D    = useFramepackTopBottom3D;
            settings.textureShareMode           = (int)textureShareMode;
            settings.equirectangularTexuteRes   = new Vector2IntItem(equirectangularTexuteRes);
            settings.useWarpBlend               = useWarpBlend;

            if (useWarpBlend && warpBlendManager != null) {
                WarpBlendSettings warpBlendSettings = new WarpBlendSettings();
                warpBlendSettings = warpBlendManager.GetSettings();
                settings.WarpBlendSettings = warpBlendSettings;
            }   
            if (headManager != null)        settings.HeadSettings = headManager.GetSettings();

            DisplayItem[] displayItems = new DisplayItem[displays.Count];
            for (int i = 0; i < displays.Count; i++){
                displayItems[i] = displays[i].GetSettings();
            }
            settings.Displays = displayItems;
            return settings;
        }
        
        /// <summary>
        /// Removes current Dispays and creates new Displays based on 
        /// DisplayItem array
        /// </summary>
        /// <param name="displayItems"></param>
        public void CreateDisplays(DisplayItem[] displayItems) {
            RemoveDisplays();

            //displays = new List<Display>();

            for (int i = 0; i < displayItems.Length; i++) {
                //GameObject displayObj = new GameObject(sharingName+ (i+1).ToString());
                GameObject displayObj = new GameObject("Display: " +sharingName+ (i+1).ToString());
                displayObj.transform.parent = this.transform;
                VirtualDisplay vd = displayObj.AddComponent<VirtualDisplay>();
                vd.displayManager = this;
                vd.headManager = headManager;
                if (cameraPrefab) vd.cameraPrefab = cameraPrefab;
                if (useCompositeTexture) displayItems[i].isRenderTextures = false;
                if (useCubemapToEqui) displayItems[i].isFisheye = false;

                // If Display Name is not set then use DisplaySettings Name + index
                if (string.IsNullOrEmpty(displayItems[i].Name)) displayItems[i].Name = sharingName + (i + 1).ToString();
                vd.SetSettings(displayItems[i]);
                vd.SetupDisplay();
                displays.Add(vd);
            }
        }


        /// <summary>
        /// Destroys all Display objects
        /// </summary>
        public void RemoveDisplays() {
            if (displays != null) {
                foreach (Display display in displays){
                    if (display != null) DestroyImmediate(display.gameObject);
                }
                displays.Clear();
            }
        }

        /// <summary>
        /// Sets the enabled state for all display cameras
        /// </summary>
        /// <param name="state"></param>
        public void SetDisplaysEnabled(bool state) {
            displays.ForEach((t) => {
                t.SetRendering = state;
            });
        }


        /// <summary>
        /// Sets near clipping plane distance for all cameras
        /// and will also save the settings
        /// </summary>
        /// <param name="distance"></param>
        //public void SetNearClip(float distance)
        //{
        //    displays.ForEach((t) => {
        //        if (t != null) t.NearClipPlane = distance;
        //    });
        //}
        public void SetNearClip(float distance) {
            headManager.NearClipPlane = distance;
        }

        /// <summary>
        /// Sets far clipping plane distance for all cameras
        /// and will also save the settings
        /// </summary>
        /// <param name="distance"></param>
        /// 
        //public void SetFarClip(float distance)
        //{
        //    displays.ForEach((t) => {
        //        if (t != null) t.FarClipPlane = distance;
        //    });
        //}
        public void SetFarClip(float distance) {
            headManager.FarClipPlane = distance;
        }

        public void SetEyeSeparation(float distance) {
            headManager.SetEyeSeparation(distance);
        }

        /// <summary>
        /// 
        /// </summary>
        public float HorizontalFOV {
            get { return horizontalFOV;}
            set {
                if (value < 1 || value > 360) return;
                horizontalFOV = value;
                if (cubeToEquiMatLeft) cubeToEquiMatLeft.SetFloat("_HorizontalFov", horizontalFOV * Mathf.Deg2Rad);
                if (cubeToEquiMatCenter) cubeToEquiMatCenter.SetFloat("_HorizontalFov", horizontalFOV * Mathf.Deg2Rad);
                if (cubeToEquiMatRight) cubeToEquiMatRight.SetFloat("_HorizontalFov", horizontalFOV * Mathf.Deg2Rad);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float VerticalFOV {
            get { return verticalFOV; }
            set {
                if (value < 1 || value > 180) return;
                verticalFOV = value;
                if (cubeToEquiMatLeft) cubeToEquiMatLeft.SetFloat("_VerticalFov", verticalFOV * Mathf.Deg2Rad);
                if (cubeToEquiMatCenter) cubeToEquiMatCenter.SetFloat("_VerticalFov", verticalFOV * Mathf.Deg2Rad);
                if (cubeToEquiMatRight) cubeToEquiMatRight.SetFloat("_VerticalFov", verticalFOV * Mathf.Deg2Rad);
            }
        }

        /// <summary>
        /// Gets all cameras from all displays. Useful for modifying camera properties
        /// </summary>
        /// <returns>List of cameras</returns>
        public List<Camera> GetCameras() {
            List<Camera> cameras = new List<Camera>();
            displays.ForEach((t) => {
                if (t != null) {
                    cameras.AddRange(t.GetCameras());
                }
            });
            return cameras;
        }

        private void Awake() {
            if (Application.isPlaying && transform.parent == null && IglooManager.Instance.dontDestroyOnLoad) DontDestroyOnLoad(this.gameObject);
        }

        /// <summary>
        /// Setup materials for shader pass
        /// </summary>
        private void SetupCubemapToEqui() {
            if (cubeToEquiMatLeft)      DestroyImmediate(cubeToEquiMatLeft);
            if (cubeToEquiMatCenter)    DestroyImmediate(cubeToEquiMatCenter);
            if (cubeToEquiMatRight)     DestroyImmediate(cubeToEquiMatRight);

            for (int i = 0; i < displays.Count; i++) {
                Dictionary<Igloo.EYE, Camera> cams = displays[i].GetActiveCameras();
                foreach (var cam in cams) {
                    if (cam.Key == Igloo.EYE.LEFT) {
                        if (cubeToEquiMatLeft == null) {
                            outputTextureLeft = new RenderTexture(equirectangularTexuteRes.x, equirectangularTexuteRes.y, 0){
                                name = "outputTextureLeft", wrapMode = TextureWrapMode.Repeat
                            };
                            cubeToEquiMatLeft = new Material(Shader.Find("Hidden/Igloo/Equirectangular"));
                            cubeToEquiMatLeft.SetFloat("_HorizontalFov", horizontalFOV * Mathf.Deg2Rad);
                            cubeToEquiMatLeft.SetFloat("_VerticalFov", verticalFOV * Mathf.Deg2Rad);
                        }
                        cubeToEquiMatLeft.SetTexture(GetFaceName((int)displays[i].iglooCubemapFace), displays[i].leftTexture);
                    }
                    else if (cam.Key == Igloo.EYE.CENTER) {
                        if (cubeToEquiMatCenter == null) {
                            outputTextureCenter = new RenderTexture(equirectangularTexuteRes.x, equirectangularTexuteRes.y, 0){
                                name = "outputTextureCenter", wrapMode = TextureWrapMode.Repeat
                            };
                            cubeToEquiMatCenter = new Material(Shader.Find("Hidden/Igloo/Equirectangular"));
                            cubeToEquiMatCenter.SetFloat("_HorizontalFov", horizontalFOV * Mathf.Deg2Rad);
                            cubeToEquiMatCenter.SetFloat("_VerticalFov", verticalFOV * Mathf.Deg2Rad);
                        }
                        cubeToEquiMatCenter.SetTexture(GetFaceName((int)displays[i].iglooCubemapFace), displays[i].centerTexture);
                    }
                    else if (cam.Key == Igloo.EYE.RIGHT) {
                        if (cubeToEquiMatRight == null) {
                            outputTextureRight = new RenderTexture(equirectangularTexuteRes.x, equirectangularTexuteRes.y, 0){
                                name = "outputTextureRight", wrapMode = TextureWrapMode.Repeat
                            };
                            cubeToEquiMatRight = new Material(Shader.Find("Hidden/Igloo/Equirectangular"));
                            cubeToEquiMatRight.SetFloat("_HorizontalFov", horizontalFOV * Mathf.Deg2Rad);
                            cubeToEquiMatRight.SetFloat("_VerticalFov", verticalFOV * Mathf.Deg2Rad);
                        }
                        cubeToEquiMatRight.SetTexture(GetFaceName((int)displays[i].iglooCubemapFace), displays[i].rightTexture);
                    }
                }
            }
            if (useFramepackTopBottom3D && outputTextureLeft != null && outputTextureRight != null) {
                int totalWidth  = 0;
                int totalHeight = 0;
                totalWidth = outputTextureLeft.width > outputTextureRight.width ? outputTextureLeft.width : outputTextureRight.width;
                totalHeight = outputTextureLeft.height + outputTextureRight.height;

                topBottom3DTexture = new RenderTexture(totalWidth, totalHeight, 0) {
                    name = "topBottomSharingTexture", wrapMode = TextureWrapMode.Repeat
                };
                topBottomMaterial = new Material(Shader.Find("Hidden/Igloo/CombineTopBottom"));
                topBottomMaterial.SetTexture("Texture1", outputTextureLeft);
                topBottomMaterial.SetTexture("Texture2", outputTextureRight);
            }
        }
       
        private string GetFaceName(int i) {
            string name = "null";
            if (i == 0) name = "_FaceTexPZ";
            else if (i == 1) name = "_FaceTexPX";
            else if (i == 2) name = "_FaceTexNZ";
            else if (i == 3) name = "_FaceTexNX";
            else if (i == 4) name = "_FaceTexNY";
            else if (i == 5) name = "_FaceTexPY"; 
            return name;
        }
    
        private void SetupComposition() {
            int totalWidth   = 0;
            int totalHeight  = 0;
            // Calculate the resoltion required for the compsition texture
            foreach (Display display in displays) {
                totalWidth += display.RenderTextureSize.x;
                if (display.RenderTextureSize.y > totalHeight)  totalHeight = display.RenderTextureSize.y;
            }
            Debug.Log("Igloo:DisplayManager:SetupComposite:  combined texture size: " + totalWidth + " , " + totalHeight);

            if (totalWidth < 1 || totalHeight < 1) return;

            float currentX = 0;
            float currentY = 0;
            for (int i = 0; i < displays.Count; i++) {
                float w = displays[i].RenderTextureSize.x / (float)totalWidth;
                float h = displays[i].RenderTextureSize.y / (float)totalHeight;
                float x = currentX;                
                float y = currentY;

                displays[i].ViewportRect = new Rect(x, y, w, h);

                // Create composition textures and assing them as appropriate camera render targets
                Dictionary<Igloo.EYE, Camera> cams = displays[i].GetActiveCameras();
                foreach (var cam in cams) {
                    if (cam.Key == Igloo.EYE.LEFT) {
                        if (outputTextureLeft == null) {
                            outputTextureLeft = new RenderTexture(totalWidth, totalHeight, 0){
                                name = "outputTextureLeft", wrapMode = TextureWrapMode.Repeat
                            };
                        }
                        cam.Value.targetTexture = outputTextureLeft;
                    } 
                    else if (cam.Key == Igloo.EYE.CENTER) {
                        if (outputTextureCenter == null) {
                            outputTextureCenter = new RenderTexture(totalWidth, totalHeight, 0){
                                name = "outputTextureCenter", wrapMode = TextureWrapMode.Repeat
                            };
                        }
                        cam.Value.targetTexture = outputTextureCenter;
                    } 
                    else if (cam.Key == Igloo.EYE.RIGHT) {
                        if (outputTextureRight == null) {
                            outputTextureRight = new RenderTexture(totalWidth, totalHeight, 0){
                                name = "outputTextureRight", wrapMode = TextureWrapMode.Repeat
                            };
                        }
                        cam.Value.targetTexture = outputTextureRight;
                    }     
                }
                currentX += w;
            }

            if (useFramepackTopBottom3D && outputTextureLeft != null && outputTextureRight != null) {
                topBottom3DTexture = new RenderTexture(totalWidth, totalHeight * 2, 0){
                    name = "topBottomSharingTexture"
                };
                topBottomMaterial = new Material(Shader.Find("Hidden/Igloo/CombineTopBottom"));
                topBottomMaterial.SetTexture("Texture1", outputTextureLeft);
                topBottomMaterial.SetTexture("Texture2", outputTextureRight);
            }
        }

        private void SetupTextureSharing() {
            TextureShareUtility.RemoveAllSendersFromObject(this.gameObject);

            if (topBottom3DTexture != null) {
                TextureShareUtility.AddTextureSender(textureShareMode, this.gameObject, sharingName,ref topBottom3DTexture);
            }
            else {
                if (useWarpBlend) warpBlendManager.enabled = true;
                
                if (outputTextureLeft != null) {
                    TextureShareUtility.AddTextureSender(textureShareMode, this.gameObject, sharingName + "_Left",ref outputTextureLeft);
                    if (useWarpBlend) warpBlendManager.canvasLeft = outputTextureLeft;
                }
                if (outputTextureCenter != null) {
                    TextureShareUtility.AddTextureSender(textureShareMode, this.gameObject, sharingName, ref outputTextureCenter);
                    if (useWarpBlend) warpBlendManager.canvasLeft = outputTextureCenter;
                }
                if (outputTextureRight != null) {
                    TextureShareUtility.AddTextureSender(textureShareMode, this.gameObject, sharingName + "_Right",ref outputTextureRight);
                    if (useWarpBlend) warpBlendManager.canvasRight = outputTextureRight;
                }
                // Enable warping and blending
                if (useWarpBlend) {
                    warpBlendManager.enabled = true;
                    warpBlendManager.canvasCentre = outputTextureCenter;
                    warpBlendManager.Setup();
                }
            } 
        }

        private void LateUpdate() {
            if (useCubemapToEqui) {
                if (cubeToEquiMatLeft != null && outputTextureLeft != null) Graphics.Blit(null, outputTextureLeft, cubeToEquiMatLeft);
                if (cubeToEquiMatCenter != null && outputTextureCenter != null) Graphics.Blit(null, outputTextureCenter, cubeToEquiMatCenter);
                if (cubeToEquiMatRight != null && outputTextureRight != null) Graphics.Blit(null, outputTextureRight, cubeToEquiMatRight);
            }

            if (useFramepackTopBottom3D) {
                if (topBottom3DTexture != null && outputTextureLeft != null && outputTextureRight != null) {
                    Graphics.CopyTexture(outputTextureRight, 0, 0, 0, 0, outputTextureRight.width, outputTextureRight.height, topBottom3DTexture, 0, 0, 0, 0);
                    Graphics.CopyTexture(outputTextureLeft, 0, 0, 0, 0, outputTextureLeft.width, outputTextureLeft.height, topBottom3DTexture, 0, 0, 0, outputTextureRight.height);
                }
            }
        }

        private void OnDestroy() {
            RemoveDisplays();
            if (headManager != null) DestroyImmediate(headManager.gameObject);
        }

    }
}

