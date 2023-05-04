using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace Igloo{

    /// <summary>
    /// Composits all the warp and blend outputs that have the same target display.
    /// One intance of WarpBlendCompositor correlates to one output display. 
    /// There should never be more than one per display.
    /// 
    /// </summary>
    [System.Serializable]
    public class WarpBlendCompositor : MonoBehaviour
    {

        public Camera displayCamera;
        public List<WarpBlend> warpBlends;
        public RenderTexture canvasCentre;
        public RenderTexture canvasLeft;
        public RenderTexture canvasRight;
         public enum RenderingMode {TEXTURE,CAMERA}
         public RenderingMode renderingMode = RenderingMode.TEXTURE;
        public void AddWarpBlend(WarpBlend warpBlend) {    
            warpBlends.Add(warpBlend);   
            warpBlend.gameObject.name = "["+warpBlends.Count.ToString()+"] " + warpBlend.gameObject.name;
            warpBlend.gameObject.transform.parent = this.transform;
        }
        public void ClearWarpBlends() { warpBlends.Clear(); }
        public void SetTargetDisplay(int index) { if (displayCamera != null) { displayCamera.targetDisplay = index; } targetDisplay = index; }
        public int GetTargetDisplay() {
            if (displayCamera != null) { return displayCamera.targetDisplay; }
            else{
                Debug.LogWarning("WarpBlendCompositor: Camera missing on game object. Can't get Target Display");
                return 0;
            }
        }
        private int targetDisplay = 0;

        public WarpBlendCompositor(int display){
            targetDisplay = display;        
            warpBlends = new List<WarpBlend>();
        }
        public WarpBlendCompositor()
        {
            warpBlends = new List<WarpBlend>();
        }
        public void Start()
        {
            if (displayCamera == null) CreateCamera(targetDisplay);
        }

        private void CreateCamera(int targetDisplay = 0){
            
            displayCamera = gameObject.GetComponent<Camera>();
            if (displayCamera == null)
                displayCamera = gameObject.AddComponent<Camera>();

            displayCamera.depth = 20;
            displayCamera.targetDisplay = targetDisplay;  
        }
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
         
            switch (renderingMode)
            {
                case RenderingMode.CAMERA:
                    for (int i = 0; i < warpBlends.Count; i++){
                        Graphics.Blit(source, destination, warpBlends[i].warpMat);
                    }
                    break;
                case RenderingMode.TEXTURE:
                    for (int i = 0; i < warpBlends.Count; i++){
                        if (warpBlends[i].debugTex != null) Graphics.Blit(warpBlends[i].debugTex, destination, warpBlends[i].warpMat);
                         else Graphics.Blit(canvasCentre, destination, warpBlends[i].warpMat);          
                    }
                    break;
                default:
                    break;
            }

        }
    }
}
