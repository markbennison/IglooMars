using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace Igloo {
    [System.Serializable]
    public struct WarpDisplay {
        public GameObject go;
        public int dispayIndex; // as per IglooWarper settings
        public int windowWidth;
        public int windowHeight;
        public int targetDisplay; // as per Windows display settings
        public int targetDisplay2; // for dual window 3D mode
        public float canvasPosU;
        public float canvasWidth;
        public float overlapLeft;
        public float overlapRight;
        public int stereoMode;
        public string warpImagesPath;
        public float positionX;
        public float positionY;
        public float scaleX;
        public float scaleY;
    }

    public class WarpBlendManager : MonoBehaviour 
        {
        public RenderTexture canvasCentre;
        public RenderTexture canvasLeft;
        public RenderTexture canvasRight;

        public List<WarpDisplay> displays;
        public List<WarpBlendCompositor> WarpCompositors;
        public string warperDataPath;
        WarpBlendSettings settings = null;
        bool isWarpSettingsLoaded = false;
        

        bool debug = false;
        public Texture2D debugTex;
        public Material unlitMat;
        public void SetSettings(WarpBlendSettings s) {
            if (s != null) {
                warperDataPath = s.warperDataPath;
                settings = s;
            } else {
                settings = new WarpBlendSettings();
                settings.Windows = new WarpWindowItem[0];
            }
        }

        public WarpBlendSettings GetSettings() {
            if (isWarpSettingsLoaded) {
                settings = new WarpBlendSettings();
                settings.warperDataPath = warperDataPath;
                WarpWindowItem[] windows = new WarpWindowItem[displays.Count];
                for (int i = 0; i < displays.Count; i++) {
                    WarpWindowItem disp = new WarpWindowItem();
                    disp.targetDisplayPrimary = displays[i].targetDisplay;
                    disp.targetDisplaySecondary = displays[i].targetDisplay2;
                    disp.positionX = displays[i].positionX;
                    disp.positionY = displays[i].positionY;
                    disp.scaleX = displays[i].scaleX;
                    disp.scaleY = displays[i].scaleY;
                    disp.stereoMode = displays[i].stereoMode;
                    windows[i] = disp;
                }
                settings.Windows = windows;
            } 
            return settings;
        }

        public WarpBlendCompositor GetDisplayCompositor(int targetDisplay){
            //Returns null if compositor does not exists
            for (int i = 0; i < WarpCompositors.Count; i++)
            {
                if (WarpCompositors[i].GetTargetDisplay() == targetDisplay)
                {
                    return WarpCompositors[i];
                }
            }

            return null;
        }

        public WarpBlendCompositor CreateWarpCompositor(int targetDisplay){
            WarpBlendCompositor newCompositor = GetDisplayCompositor(targetDisplay);
          if (newCompositor != null){
                //Compositor already exists for this display
                Debug.LogWarning("<WarpBlendManager> Display Compositor already exists for display :" + targetDisplay);
          }else{
                //Create new GameObject with a compositor and add to list
                GameObject go = new GameObject();
                go.transform.parent = this.transform;
                go.name = "Warp&Blend Display Output: " + targetDisplay.ToString();
                go.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                Camera newCam = go.AddComponent<Camera>();
                newCompositor = go.AddComponent<WarpBlendCompositor>();
                newCompositor.displayCamera = newCam;
                newCompositor.SetTargetDisplay(targetDisplay);
                newCompositor.canvasCentre = canvasCentre;
                newCompositor.canvasLeft = canvasLeft;
                newCompositor.canvasRight = canvasRight;
                WarpCompositors.Add(newCompositor);
            }
            return newCompositor;
        }
        public void AddWarpBlendToCompositor(WarpBlend warpBlend){
            int targetDisplay = warpBlend.targetDisplay;

            WarpBlendCompositor displayCompositor = GetDisplayCompositor(targetDisplay);
            if(displayCompositor == null){
                //Create new Compositor if one doesn't exists for the targetDisplay
                displayCompositor = CreateWarpCompositor(targetDisplay);
            }

            displayCompositor.AddWarpBlend(warpBlend);

        }

        public void Setup()
        {
            if (string.IsNullOrEmpty(warperDataPath))
            {
                warperDataPath = Utils.GetWarperDataPath();
            }
            if (!Directory.Exists(warperDataPath))
            {
                Debug.LogError("Warper settings path does not exist: " + warperDataPath);
                return;
            }

            isWarpSettingsLoaded = LoadWarperSettings();
            if (isWarpSettingsLoaded) SetupDisplays();
        }
        private bool LoadWarperSettings() {
            Debug.Log("Loading warper settings files from: " + warperDataPath);
            // Load Canvas Settings
            string mainPath = Path.Combine(warperDataPath, "IglooWarperMainSettings.xml");
            XmlDocument mainDoc = new XmlDocument();

            // Load Blend Settings
            string blendPath = Path.Combine(warperDataPath, "BlenderSettings.xml");
            XmlDocument blendDoc = new XmlDocument();

            if (File.Exists(mainPath) && File.Exists(blendPath)) {
                displays = new List<WarpDisplay>();

                // The Warper main settings does not have a single root node so manually add one
                string xmlString = System.IO.File.ReadAllText(mainPath);
                XDocument xdoc = XDocument.Parse("<root>" + xmlString + "</root>");
                mainDoc = new XmlDocument();
                mainDoc = ToXmlDocument(xdoc);

                blendDoc.Load(blendPath);
                var displayElements = mainDoc.GetElementsByTagName("display");
                int numDisplays = displayElements.Count;

                // number of displays changed
                if (settings.Windows.Length != numDisplays) settings.Windows = new WarpWindowItem[0];

                for (int i = 0; i < numDisplays; i++) {
                    WarpDisplay disp = new WarpDisplay();

                    // Set setting from IglooSetting  , 
                    if (i < settings.Windows.Length) {
                        disp.targetDisplay = settings.Windows[i].targetDisplayPrimary;
                        disp.targetDisplay2 = settings.Windows[i].targetDisplaySecondary;
                        disp.stereoMode = settings.Windows[i].stereoMode;
                        disp.positionX = settings.Windows[i].positionX;
                        disp.positionY = settings.Windows[i].positionY;
                        disp.scaleX = settings.Windows[i].scaleX;
                        disp.scaleY = settings.Windows[i].scaleY;
                    }
                    // If no settings are found apply some defaults
                    else {
                        disp.targetDisplay = i;
                        disp.targetDisplay2 = i + numDisplays;
                        disp.scaleX = 1.0f;
                        disp.scaleY = 1.0f;
                    }
                    disp.dispayIndex = i;
                    disp.warpImagesPath = Path.Combine(warperDataPath, "warps\\high performance");

                    // Get Canvas Settings
                    var canvasUAttrib = displayElements[i].Attributes["canvasPosU"];
                    if (canvasUAttrib != null) {
                        float canvasU;
                        if (float.TryParse(canvasUAttrib.Value, out canvasU)) disp.canvasPosU = canvasU;
                    }
                    var canvasWidthAttrib = displayElements[i].Attributes["canvasSubsectionWidth"];
                    if (canvasWidthAttrib != null) {
                        float canvasWidth;
                        if (float.TryParse(canvasWidthAttrib.Value, out canvasWidth)) disp.canvasWidth = canvasWidth;
                    }

                    // Get Blend Settings
                    string leftTag = "leftBlendingOverlap-" + i.ToString();
                    string rightTag = "rightBlendingOverlap-" + i.ToString();
                    var left = blendDoc.GetElementsByTagName(leftTag);
                    if (left.Count > 0) {
                        float leftBlend;
                        if (float.TryParse(left[0].InnerText, out leftBlend)) disp.overlapLeft = leftBlend / 100.0f;
                        var right = blendDoc.GetElementsByTagName(rightTag);
                        float rightBlend;
                        if (float.TryParse(right[0].InnerText, out rightBlend)) disp.overlapRight = rightBlend / 100.0f;
                        displays.Add(disp);
                        Debug.Log("Warp display settings loaded, index: " + i + " , Canvas U: " + disp.canvasPosU + " , canvas Width: " + disp.canvasWidth + " , left overlap:" + disp.overlapLeft + " , right overlap:" + disp.overlapRight);
                    }


                }
                return true;
            } 
            else {
                Debug.LogError("Igloo Warper settings could not be found");
                return false;
            } 
        }

        private void SetupDisplays() {
            for (int i = 0; i < displays.Count; i++) {
                // Stereo 3D - Dual window mode i.e a window per eye 
                if (displays[i].stereoMode == 3) {
                    // Left Eye
                    GameObject go = new GameObject();
                    go.name = "Warp&Blend Left Eye";// + displays[i].dispayIndex.ToString();
                    WarpBlend warp = go.AddComponent<WarpBlend>();
                    warp.centreTex = canvasLeft;
                    if (debug) warp.debugTex = debugTex;
                    warp.Setup(displays[i]);
                    warp.targetDisplay = displays[i].targetDisplay;
                    AddWarpBlendToCompositor(warp);
                    // Right Eye
                    GameObject go2 = new GameObject();
                    go2.name = "Warp&Blend Right Eye";// + displays[i].dispayIndex.ToString();
                    WarpBlend warp2 = go2.AddComponent<WarpBlend>();
                    warp2.centreTex = canvasRight;
                    if (debug) warp2.debugTex = debugTex;
                    warp2.Setup(displays[i]);
                    warp2.targetDisplay = displays[i].targetDisplay;
                    AddWarpBlendToCompositor(warp2);

                } 
                // Single window - either not stereo (0) , side-by-side (1) stereo or top-bottom stereo (2)
                else {
                    GameObject go = new GameObject();
                    go.transform.parent = this.transform;
                    go.name = "Warp&Blend";// + displays[i].dispayIndex.ToString();
                    WarpBlend warp =  go.AddComponent<WarpBlend>();
                    warp.centreTex = canvasCentre;
                    warp.leftEyeTex = canvasLeft;
                    warp.rightEyeTex = canvasRight;
                    if (debug) warp.debugTex = debugTex;
                    warp.Setup(displays[i]);
                    warp.targetDisplay = displays[i].targetDisplay;
                    AddWarpBlendToCompositor(warp);
                }
            }
        }

        public XmlDocument ToXmlDocument(XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using(var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }
    }
}


