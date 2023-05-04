using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Igloo {
    public class PhysicalDisplay : Display
    {
        public int      targetDisplay;
        public RectInt  windowBounds;
        public RectInt  leftWindowViewport;
        public RectInt  rightWindowViewport;
        public bool     exclusiveFullscreen;

        private void Start() {
        }

        public override void SetSettings(DisplayItem settings) {
            base.SetSettings(settings);
            //viewPortRect = new Rect(settings.viewportRect.x, settings.viewportRect.y, settings.viewportRect.w, settings.viewportRect.h);
        }
        
        public override DisplayItem GetSettings() {
            DisplayItem settings = base.GetSettings();
            //RectItem viewportRectItem = new RectItem();
            //viewportRectItem.x = viewPortRect.x;
            //viewportRectItem.y = viewPortRect.y;
            //viewportRectItem.w = viewPortRect.width;
            //viewportRectItem.h = viewPortRect.height;
            //settings.viewportRect = viewportRectItem;
        
            return settings;
        }
        
        public override void InitialiseCameras() {
            base.InitialiseCameras();
            //Camera leftCam = headManager.CreateLeftEye(name, isOffAxis ? Vector3.zero : camRotation);
            //if (is3D) leftCam.stero;
        }
    }
}

