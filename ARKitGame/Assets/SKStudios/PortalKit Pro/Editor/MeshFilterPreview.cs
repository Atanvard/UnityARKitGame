using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace SKStudios.Portals.Editor
{
    [CustomEditor(typeof(MeshFilter))]
    [CanEditMultipleObjects]
    public class MeshFilterPreview : UnityEditor.Editor {

        private PreviewRenderUtility _previewRenderUtility;

        private MeshFilter _targetMeshFilter;
        public virtual MeshFilter TargetMeshFilter {
            get { return _targetMeshFilter; }
            set { _targetMeshFilter = value; }
        }
        private MeshRenderer _targetMeshRenderer;
        public virtual MeshRenderer TargetMeshRenderer {
            get { return _targetMeshRenderer; }
            set { _targetMeshRenderer = value; }
        }
        private Vector2 _drag;

        private void ValidateData() {
            if (_previewRenderUtility == null) 
                _previewRenderUtility = new PreviewRenderUtility();
           



            if (!TargetMeshFilter)
                TargetMeshFilter = target as MeshFilter;

            if(!TargetMeshRenderer && TargetMeshFilter)
                TargetMeshRenderer = TargetMeshFilter.GetComponent<MeshRenderer>();
        }

        public bool HasPreviewGUI_s() {
            ValidateData();

            return true;
        }
        Light[] lights = new Light[2];
        public void OnPreviewGUI_s(Rect r, GUIStyle background) {
            _drag = Drag2D(_drag, r);



            if (Event.current.type == EventType.Repaint) {
                if (TargetMeshRenderer == null) {
                    EditorGUI.DropShadowLabel(r, "Mesh Renderer Required");
                }
                else {
                    if (_previewRenderUtility == null)
                        _previewRenderUtility = new PreviewRenderUtility();
                    /*
                    if(!lights[0])
                        lights[0] = LightObj.AddComponent<Light>();
                    lights[0].color = new Color(1, 0.9f, 0.8f);
                    lights[0].transform.rotation = Quaternion.Euler(90f, 90f, 0f);
                    lights[0].intensity = 1f;
                    lights[0].transform.position = new Vector3(0, 1, -1);
                    
                    //InternalEditorUtility.SetCustomLighting(lights, Color.black);
                   */

                    _previewRenderUtility.BeginPreview(r, null);
                    TargetMeshFilter.transform.localPosition = new Vector3(0, 0.181f, -0.383f);
                    TargetMeshFilter.transform.localRotation = Quaternion.identity;
                    TargetMeshFilter.transform.localScale = Vector3.one;

                    Transform targetTransform = ((Component) target).transform;

                    for (int i = 0; i < TargetMeshRenderer.sharedMaterials.Length; i++) {
                        _previewRenderUtility.DrawMesh(TargetMeshFilter.sharedMesh, targetTransform.localToWorldMatrix,TargetMeshRenderer.sharedMaterials[i], i);
                    }

                    _previewRenderUtility.camera.transform.position =(targetTransform.position - targetTransform.forward * 0.6f) - targetTransform.up * 0.3f + targetTransform.forward * 0.4f;
                    
                    _previewRenderUtility.camera.farClipPlane = 1000;
                    _previewRenderUtility.camera.fieldOfView = 20f;
                   _previewRenderUtility.camera.ResetProjectionMatrix();

                    _previewRenderUtility.camera.transform.rotation =
                        Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
                    _previewRenderUtility.camera.transform.position +=
                        _previewRenderUtility.camera.transform.forward * -6f;
                    
                    _previewRenderUtility.camera.Render();

                    

                    Texture resultRender = _previewRenderUtility.EndPreview();
                    GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
                }

            }
        }

        public override void OnPreviewSettings() {
            if (GUILayout.Button("Reset Camera", EditorStyles.whiteMiniLabel))
                _drag = Vector2.zero;
        }

        void OnDestroy() {
            if(_previewRenderUtility != null)
                _previewRenderUtility.Cleanup();
        }

        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position) {
            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && position.width > 50f) {
                        GUIUtility.hotControl = controlID;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID) {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID) {
                        scrollPosition -= current.delta * (float) ((!current.shift) ? 1 : 3) /
                                          Mathf.Min(position.width, position.height) * 140f;
                        scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
                        current.Use();
                        GUI.changed = true;
                    }
                    break;
            }
            return scrollPosition;
        }

    }
}