
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace SKStudios.Portals {
    public static class TagDatabase {
        public static Dictionary<string, Dictionary<string, string>> tags = new Dictionary<string, Dictionary<string, string>>()
        {
{"Fallback", new Dictionary<string, string>(){}},
{"HTCVive", new Dictionary<string, string>(){}},
{"OculusTouch_Left", new Dictionary<string, string>(){}},
{"OculusTouch_Right", new Dictionary<string, string>(){}},
{"Simulator", new Dictionary<string, string>(){}},
{"SteamVROculusTouch_Left", new Dictionary<string, string>(){}},
{"SteamVROculusTouch_Right", new Dictionary<string, string>(){}},
{"XimmerseCobra02", new Dictionary<string, string>(){}},
{"CirclePortal", new Dictionary<string, string>(){}},
{"InverseBox", new Dictionary<string, string>(){}},
{"PortalEffect", new Dictionary<string, string>(){}},
{"InvisiblePortal", new Dictionary<string, string>(){{"PortalTarget","Portal"},{"PortalSource","Portal"},{"PortalCamera","Portal"},{"PortalCameraParent","Portal"},{"PortalPhysicsPassthrough","Portal"},{"PortalTrigger","Portal"},{"BackWall","Portal"},{"PortalEdgeWall","Portal"},{"PortalPlaceholder","PortalPlaceholder"},{"PortalRenderer","Portal"},{"PortalOpeningSize","Portal"},{"InvisiblePortal","Portal"}}},
{"RectPortal", new Dictionary<string, string>(){{"PortalTarget","Portal"},{"PortalSource","Portal"},{"PortalCamera","Portal"},{"PortalCameraParent","Portal"},{"Physics Passthrough Scanner","Portal"},{"PortalPhysicsPassthrough","Portal"},{"PortalTrigger","Portal"},{"BackWall","Portal"},{"PortalPlaceholder","PortalPlaceholder"},{"EdgeWall","Portal"},{"EdgeWall 2","Portal"},{"EdgeWall 3","Portal"},{"EdgeWall 4","Portal"},{"EdgeWall 4 (1)","Portal"},{"PortalEdgeWall","Portal"},{"PortalRenderer","Portal"},{"PortalOpeningSize","Portal"},{"RectPortal","Portal"}}},
{"RectPortalNoBuffer", new Dictionary<string, string>(){{"PortalTarget","Portal"},{"PortalSource","Portal"},{"PortalCamera","Portal"},{"PortalCameraParent","Portal"},{"Physics Passthrough Scanner","Portal"},{"PortalPhysicsPassthrough","Portal"},{"PortalTrigger","Portal"},{"BackWall","Portal"},{"PortalPlaceholder","PortalPlaceholder"},{"PortalEdgeWall","Portal"},{"SeamlessRecursionFix","Portal"},{"PortalRenderer","Portal"},{"PortalOpeningSize","Portal"},{"RectPortalNoBuffer","Portal"}}},
{"PortalSpawner", new Dictionary<string, string>(){{"PortalSpawner","Portal"}}},
{"[CameraRig]", new Dictionary<string, string>(){}},
{"PortalKitPro VRTK Starter", new Dictionary<string, string>(){{"Camera (eye)","Player"}}},
{"Arc", new Dictionary<string, string>(){}}
};
        }
}