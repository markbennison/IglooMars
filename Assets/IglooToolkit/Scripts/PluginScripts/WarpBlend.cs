using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace Igloo {

    public class WarpBlend : MonoBehaviour
    {
        private int displayIndex = 0;
        public int DisplayIndex {  get {return displayIndex; } set {displayIndex = value; UpdateShaderProperties();} }
        private float canvasPosU = 0.0f;
        public float CanvasPosU {  get {return canvasPosU; } set {canvasPosU = value; UpdateShaderProperties();} }
        private float canvasWidth = 1.0f;
        public float CanvasWidth {  get {return canvasWidth; } set {canvasWidth = value; UpdateShaderProperties();} }
        private float overlapLeft = 0.0f;
        public float OverlapLeft {  get {return overlapLeft; } set {overlapLeft = value; UpdateShaderProperties();} }
        private float overlapRight = 0.0f;
        public float OverlapRight {  get {return overlapRight; } set {overlapRight = value; UpdateShaderProperties(); } }
        private float stereoMode = 0.0f;
        public float StereoMode { get { return stereoMode; } set { stereoMode = value; UpdateShaderProperties(); } }

        private float displayPosX;
        public float DisplayPosX { get { return displayPosX; } set { displayPosX = value; UpdateShaderProperties(); } }
        private float displayPosY;
        public float DisplayPosY { get { return displayPosY; } set { displayPosY = value; UpdateShaderProperties(); } }
        private float displayScaleX;
        public float DisplayScaleX { get { return displayScaleX; } set { displayScaleX = value; UpdateShaderProperties(); } }
        private float displayScaleY;
        public float DisplayScaleY { get { return displayScaleY; } set { displayScaleY = value; UpdateShaderProperties(); } }

        private int windowWidth = 1920;
        private int windowHeight = 1080;
        
        public int targetDisplay = 0;


        public Texture2D warpTex, blendTex; 
        public Texture2D debugTex = null;
        public RenderTexture centreTex, leftEyeTex, rightEyeTex;
        public Material warpMat;
       
        public string warpImagesPath = "C:/Users/Harvey-PC/AppData/Local/Igloo Vision/IglooWarper/warps/high performance";

        public void Setup(WarpDisplay display) {                
            CanvasPosU  = display.canvasPosU;
            CanvasWidth = display.canvasWidth;
            OverlapLeft = display.overlapLeft;
            OverlapRight= display.overlapRight;
            DisplayIndex = display.dispayIndex;
            StereoMode  = display.stereoMode;
            DisplayPosX = display.positionX;
            DisplayPosY = display.positionY;
            DisplayScaleX = display.scaleX;
            DisplayScaleY = display.scaleY;
            warpImagesPath = display.warpImagesPath;
            if (display.windowWidth > 0)    windowWidth     = display.windowWidth;
            if (display.windowHeight > 0)   windowHeight    = display.windowHeight;
            ApplyWarpAndBlend(warpImagesPath, displayIndex+1, true);
            UpdateShaderProperties();

            
        }

        private void ApplyWarpAndBlend(string folderPath, int screenIndex, bool isLoadBlend = false) {
            string warpPath = Path.Combine(folderPath, "Screen"+ screenIndex.ToString() +"-warp32.bmp");
            string blendPath = Path.Combine(folderPath, "Screen"+ screenIndex.ToString() + "-edgeBlend.png");

            warpMat = new Material(Shader.Find("Igloo/Warp"));
           
            if (isLoadBlend) {
                blendTex = loadTexture(blendPath);
                if (blendTex != null) {
                    warpMat.SetTexture("_BlendTex", blendTex);
                    windowWidth = blendTex.width;
                    windowHeight = blendTex.height;
                }
            }
            //warpMat.renderQueue = 3000;
            warpTex = loadFloatTexture(warpPath, windowWidth ,windowHeight ,54);

            if (warpTex != null) warpMat.SetTexture("_WarpTex", warpTex);
            else {
                Debug.LogError("Failed to load Warp texture from " + warpPath);
            }

            // side-by-side or top-bottom
            if (stereoMode == 1 || stereoMode == 2) {
                if (debugTex != null) {
                    warpMat.SetTexture("_LeftEyeTex", debugTex);
                    warpMat.SetTexture("_RightEyeTex", debugTex);
                } else {
                    if (leftEyeTex != null) warpMat.SetTexture("_LeftEyeTex", leftEyeTex);
                    if (rightEyeTex != null) warpMat.SetTexture("_RightEyeTex", rightEyeTex);
                }
            }
        }

        Texture2D loadTexture(string path) {
            byte[] fileData;
            Texture2D tex = null;
            if (File.Exists(path))
            {
                tex = new Texture2D(2, 2);
                fileData = File.ReadAllBytes(path);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); 
            }
            return tex;
        }

        Texture2D loadFloatTexture(string path, int w, int h, int ignoreBytes = 0) {
            Debug.Log("About to attempt loading float texture, path: " + path + " ,w: " + w + " h: " + h);
            int texWidth = w;
            int texHeight = h;
            Texture2D tex = null;

            if (!System.IO.File.Exists(path)) {
                Debug.LogWarning("Cannot find Igloo Warp Image at: " + path);
                return tex;
            }

            byte[] sArray = File.ReadAllBytes(path);

            float[] dArray = new float[(sArray.Length - ignoreBytes) / 4];

            Debug.Log("sArray size: " + sArray.Length + "  dArray size: " + dArray.Length);

            Buffer.BlockCopy(sArray, ignoreBytes, dArray, 0, sArray.Length - ignoreBytes);

            int bArrayPos = 0;
            int pixelPos = 0;

            Color[] pixels = new Color[texWidth * texHeight];

            for (int y = 0; y < texHeight; y++) {
                for (int x = 0; x < texWidth; x++) {
                    float r = dArray[bArrayPos];
                    float g = 1 - dArray[bArrayPos + 1];
                    float b = dArray[bArrayPos + 2];
                    float a = dArray[bArrayPos + 3];

                    Color pixel;
                    pixel = new Color(r, g, b, a);

                    pixels[pixelPos] = pixel;
                    bArrayPos += 4;
                    pixelPos++;
                }
            }

            tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false);
            tex.SetPixels(pixels);
            tex.Apply();

            return tex;
        }

        private void UpdateShaderProperties() {
            if (!warpMat) return; 
             warpMat.SetFloat("_canvasPosU", canvasPosU);
             warpMat.SetFloat("_canvasWidth", canvasWidth);
             warpMat.SetFloat("_overlapLeft", overlapLeft);
             warpMat.SetFloat("_overlapRight", overlapRight);
             warpMat.SetFloat("_stereoMode", stereoMode);
             warpMat.SetFloat("_displayPosX", displayPosX);
             warpMat.SetFloat("_displayPosY", displayPosY);
             warpMat.SetFloat("_displayScaleX", displayScaleX);
             warpMat.SetFloat("_displayScaleY", displayScaleY);
        }

    }
}

