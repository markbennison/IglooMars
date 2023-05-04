using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace Igloo {

    [XmlRootAttribute("IglooSettings", IsNullable = false)]
    public class Settings {
        [XmlAttribute]
        public string version;
        public string Name;
        public SystemSettings       SystemSettings;
        public PlayerSettings       PlayerSettings;
        public NetworkSettings      NetworkSettings;
        public UISettings           UISettings;
        public DisplaySettings      DisplaySettings;
        public WindowSettings       WindowSettings;
    }

    public class HeadSettings {
        public Vector3Item headPositionOffset;
        public Vector3Item headRotationOffset;
        public Vector3Item leftEyeOffset;
        public Vector3Item rightEyeOffset;
        public bool headtracking;
    }

    public class DisplaySettings {
        [XmlAttribute]
        public string Name;
        public int  textureShareMode; // 0 - none, 1 - spout, 2 - ndi
        public bool useCubemapToEquirectangular;
        public Vector2IntItem equirectangularTexuteRes;
        public float    horizontalFOV;
        public float    verticalFOV;
        public bool     useCompositeTexture;
        public bool     useFramepackTopBottom3D;
        public bool     useWarpBlend;
        public HeadSettings         HeadSettings;
        public DisplayItem[]        Displays;
        public WarpBlendSettings    WarpBlendSettings;

    }

    public class WarpWindowItem {
        public int targetDisplayPrimary;
        public int targetDisplaySecondary; // for dual window 3D mode 
        public int stereoMode; // 0 - off, 1- side by side, 2- top bottom, 3 - dual window
        public float positionX;
        public float positionY;
        public float scaleX;
        public float scaleY;
    }

    public class WarpBlendSettings {
        public string warperDataPath;
        public WarpWindowItem[]  Windows;
    }

    public class WindowSettings {
        [XmlAttribute]
        public bool enabled;
        public WindowItem[] Windows;
    }

    public class WindowItem {
        public int width;
        public int height;
        public int positionOffsetX;
        public int positionOffsetY;
    }

    public class DisplayItem {
        [XmlAttribute]
        public string           Name;
        public bool             isRendering;
        public float            fov;
        public bool             is3D;
        public bool             isRenderTextures;
        public int              cubemapFace; // 0 - left, 1 - front, 2 - right, 3 - back, 4 - down, 5 - up
        public Vector2IntItem   renderTextureSize;
        public int              textureShareMode; // 0 - none, 1 - spout, 2 - ndi
        public Vector3Item      cameraRotation;
        public float            nearClipPlane;
        public float            farClipPlane;
        public bool isFisheye;
        [XmlElement(IsNullable = false)]
        public Vector2Item fisheyeStrength;


        public bool             isOffAxis;
        [XmlElement(IsNullable = false)]
        public Vector3Item      viewportRotation;
        [XmlElement(IsNullable = false)]
        public Vector2Item      viewportSize;
        [XmlElement(IsNullable = false)]
        public Vector3Item      viewportPosition;

        public int targetDisplay = -1;
        // Physical Display
        //public RectInt windowBounds;
        //public RectInt leftWindowViewport;
        //public RectInt rightWindowViewport;
        //public bool exclusiveFullscreen;
        //public int display;
    }

    public class PlayerSettings {
        [XmlAttribute]
        public string Name;
        public bool usePlayer;
        public int rotationInput;   // 0 - standard, 1 - warper, 2 - gyrOSC , 3 - vr controller
        public int rotationMode;    // 0 - igloo360, 1 - igloo non-360, 2 - game
        public int movementInput;   // 0 - standard, 1 - gyrosc 
        public int movementMode;    // 0 - walking, 1 - flying, 2 - ghost
        public float runSpeed;
        public float walkSpeed;
        public float smoothTime;
        public int crosshairHideMode; // 0 - show, 1 - show on move, 2- hide
        public bool isCrosshair3D; 
    }

    public class NetworkSettings {
        public int      inPort;
        public int      outPort;
        public string   outIP;
    }

    public class SystemSettings {
        [XmlAttribute]
        public int vSyncMode;
        [XmlAttribute]
        public int targetFPS;
        [XmlAttribute]
        public bool sendStartupMessage;
    }

    public class UISettings {
        public bool useUI;
        public string screenName;
        public Vector3Item screenPos;
        public Vector3Item screenRot;
        public Vector3Item screenScale;
        public bool followCrosshair;
        public float followSpeed;
    }

    public class Vector2Item {
        public Vector2Item(){}
        public Vector2Item(Vector2 val) { x = val.x; y = val.y; }
        public Vector2Item(float X, float Y) { x =X; y = Y; }
        [XmlAttribute] public float x;
        [XmlAttribute] public float y;
        public Vector2 Vector2 { get { return new Vector2(x, y); } }
    }

    public class Vector2IntItem {
        public Vector2IntItem() { }
        public Vector2IntItem(Vector2Int val) { x = val.x; y = val.y;}
        public Vector2IntItem(int X, int Y) { x = X; y = Y; }
        [XmlAttribute] public int x;
        [XmlAttribute] public int y;
        public Vector2Int Vector2Int { get { return new Vector2Int(x, y); } }
    }

    public class Vector3Item {
        public Vector3Item() { }
        public Vector3Item(Vector3 val) { x = val.x; y = val.y; z = val.z; }
        public Vector3Item(float X, float Y, float Z) { x = X; y = Y; z = Z; }
        [XmlAttribute] public float x;
        [XmlAttribute] public float y;
        [XmlAttribute] public float z;
        public Vector3 Vector3 { get { return new Vector3(x,y,z);}}
    }

    public class RectItem {
        public RectItem() { }
        public RectItem(Rect val) { x = val.x; y = val.y; w = val.width; h = val.height; }
        public RectItem(float X, float Y, float W, float H) { x = X; y = Y; w = W; h = H; }
        [XmlAttribute] public float x;
        [XmlAttribute] public float y;
        [XmlAttribute] public float w;
        [XmlAttribute] public float h;
        public Rect Rect { get { return new Rect(x, y, w, h); } }
    }

    public class TransformItem {
        [XmlAttribute] public float posX;
        [XmlAttribute] public float posY;
        [XmlAttribute] public float posZ;
        [XmlAttribute] public float rotX;
        [XmlAttribute] public float rotY;
        [XmlAttribute] public float rotZ;
        [XmlAttribute] public float scaleX;
        [XmlAttribute] public float scaleY;
        [XmlAttribute] public float scaleZ;
    }

    public class Serializer {
        public Settings Load(string filename) {
            if (!Path.HasExtension(filename)) filename += ".xml";           
            try {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                serializer.UnknownNode += new
                XmlNodeEventHandler(serializer_UnknownNode);
                serializer.UnknownAttribute += new
                XmlAttributeEventHandler(serializer_UnknownAttribute);

                FileStream fs = new FileStream(filename, FileMode.Open);
                Settings settings;
                settings = (Settings)serializer.Deserialize(fs);
                fs.Close();
                return settings;
            }
            catch (SystemException e) {
                Debug.LogWarning(e);
                return null;
            }
        }

        public void Save(string filename, Settings settings) {
            if (!Path.HasExtension(filename)) filename += ".xml";
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            TextWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, settings);
            writer.Close();
        }

        private void serializer_UnknownNode (object sender, XmlNodeEventArgs e) {
            Debug.LogWarning("Unknown Node:" + e.Name + "\t" + e.Text);
        }

        private void serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e) {
            System.Xml.XmlAttribute attr = e.Attr;
            Debug.LogError("Unknown attribute " + attr.Name + "='" + attr.Value + "'");
        }
    }
}
