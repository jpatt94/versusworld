#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections.Generic;

namespace O3DWB
{
    public class PrefabPreviewGenerator
    {
        private Color _backgroundColor = new Color(0.321568638f, 0.321568638f, 0.321568638f, 1f);
        private int _previewWidth = 128;
        private int _previewHeight = 128;
        private bool _isPreiewGenSessionActive;

        private Dictionary<Light, bool> _sceneLightToActiveState = new Dictionary<Light, bool>();

        private Camera _renderCamera;
        private RenderTexture _renderTexture;

        private Light[] _previewLights;

        public static PrefabPreviewGenerator Get() { return Octave3DWorldBuilder.ActiveInstance.PrefabPreviewGenerator; }

        public void BeginPreviewGenSession()
        {
            if (_isPreiewGenSessionActive) return;

            _sceneLightToActiveState.Clear();
            List<Light> sceneLights = Octave3DScene.Get().GetSceneLights();
            foreach (var light in sceneLights)
            {
                _sceneLightToActiveState.Add(light, light.gameObject.activeSelf);
                light.gameObject.SetActive(false);
            }

            _isPreiewGenSessionActive = true;
        }

        public void EndPreviewGenSession()
        {
            if (!_isPreiewGenSessionActive) return;

            foreach (var pair in _sceneLightToActiveState)
            {
                if (pair.Key == null) continue;
                pair.Key.gameObject.SetActive(pair.Value);
            }
            _sceneLightToActiveState.Clear();

            _isPreiewGenSessionActive = false;
        }

        public Texture2D GeneratePreview(Prefab prefab)
        {
            if (prefab == null || prefab.UnityPrefab == null || !_isPreiewGenSessionActive) return null;

            Camera renderCam = GetRenderCamera();
            renderCam.backgroundColor = _backgroundColor;
            renderCam.orthographic = false;
            renderCam.fieldOfView = 65.0f;
            renderCam.targetTexture = GetRenderTexture();
            renderCam.clearFlags = CameraClearFlags.Color;
            if (renderCam.targetTexture == null) return null;

            RenderTexture oldRenderTexture = UnityEngine.RenderTexture.active;
            RenderTexture.active = renderCam.targetTexture;
            GL.Clear(true, true, _backgroundColor);

            GameObject previewObjectRoot = GameObject.Instantiate(prefab.UnityPrefab);
            previewObjectRoot.hideFlags = HideFlags.HideAndDontSave;
            Transform previewObjectTransform = previewObjectRoot.transform;
            previewObjectTransform.position = Vector3.zero;
            previewObjectTransform.rotation = Quaternion.identity;
            previewObjectTransform.localScale = Vector3.one;

            Box worldBox = previewObjectRoot.GetHierarchyWorldBox();
            Sphere worldSphere = worldBox.GetEncpasulatingSphere();

            Sphere sceneSphere = Octave3DWorldBuilder.ActiveInstance.Octave3DScene.GetEncapuslatingSphere();
            Vector3 newPreviewSphereCenter = sceneSphere.Center - Vector3.right * (sceneSphere.Radius + worldSphere.Radius + 90.0f);
            previewObjectTransform.position += (newPreviewSphereCenter - worldSphere.Center);
            worldBox = previewObjectRoot.GetHierarchyWorldBox();
            worldSphere = worldBox.GetEncpasulatingSphere();

            Transform camTransform = renderCam.transform;
            camTransform.rotation = Quaternion.identity;
            camTransform.rotation = Quaternion.AngleAxis(-45.0f, Vector3.up) * Quaternion.AngleAxis(35.0f, camTransform.right);
            camTransform.position = worldSphere.Center - camTransform.forward * (worldSphere.Radius * 2.0f + renderCam.nearClipPlane + 0.1f);

            SetPreviewLightsActive(true);
            SetupPreviewLights();
            renderCam.Render();
            SetPreviewLightsActive(false);

            /*GL.PushMatrix();
            GL.LoadProjectionMatrix(renderCam.projectionMatrix);

            List<GameObject> allPreviewObjects = previewObjectRoot.GetAllChildrenIncludingSelf();
            foreach (var previewObject in allPreviewObjects)
            {
                MeshFilter meshFilter = previewObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    Mesh mesh = meshFilter.sharedMesh;
                    if (mesh == null) continue;
                    MeshRenderer meshRenderer = previewObject.GetComponent<MeshRenderer>();
                    if (meshRenderer == null) continue;

                    Matrix4x4 worldMatrix = previewObject.transform.localToWorldMatrix;
                    Matrix4x4 modelViewMtx = renderCam.worldToCameraMatrix * worldMatrix;
                    for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; ++subMeshIndex)
                    {
                        Material material = meshRenderer.sharedMaterials[subMeshIndex];
                        if (material == null) continue;
                        material.SetPass(0);

                        //Graphics.DrawMeshNow(mesh, modelViewMtx, subMeshIndex);
                        Graphics.DrawMesh(mesh, worldMatrix, material, previewObjectRoot.layer, renderCam, subMeshIndex);
                    }
                }
            }
            GL.PopMatrix();*/

            GameObject.DestroyImmediate(previewObjectRoot);
            Texture2D previewTexture = new Texture2D(_previewWidth, _previewHeight, TextureFormat.ARGB32, true, true);
            previewTexture.ReadPixels(new Rect(0, 0, _previewWidth, _previewHeight), 0, 0);
            previewTexture.Apply();
            UnityEngine.RenderTexture.active = oldRenderTexture;

            return previewTexture;
        }

        public void DestroyData()
        {
            if (_renderCamera != null)
            {
                GameObject.DestroyImmediate(_renderCamera.gameObject);
                _renderCamera = null;
            }
            if (_renderTexture != null)
            {
                RenderTexture.DestroyImmediate(_renderTexture);
                _renderTexture = null;
            }

            if(_previewLights != null)
            {
                for (int lightIndex = 0; lightIndex < 3; ++lightIndex)
                {
                    if (_previewLights[lightIndex] != null) GameObject.DestroyImmediate(_previewLights[lightIndex].gameObject);
                    _previewLights[lightIndex] = null;
                }
                _previewLights = null;
            }
        }

        private Camera GetRenderCamera()
        {
            if(_renderCamera == null)
            {
                GameObject renderCamObject = EditorUtility.CreateGameObjectWithHideFlags("Preview Camera", HideFlags.HideAndDontSave);
                _renderCamera = renderCamObject.AddComponent<Camera>();
            }

            return _renderCamera;
        }

        private RenderTexture GetRenderTexture()
        {
            if(_renderTexture == null)
            {
                _renderTexture = new RenderTexture(_previewWidth, _previewHeight, 24);
                if (_renderTexture == null) return null;
                _renderTexture.Create();
            }

            return _renderTexture;
        }

        private Light[] GetPreviewLights()
        {
            if(_previewLights == null)
            {
                _previewLights = new Light[1];

                for(int lightIndex = 0; lightIndex < 1; ++lightIndex)
                {
                    GameObject lightObject = EditorUtility.CreateGameObjectWithHideFlags("Preview Dir Light", HideFlags.HideAndDontSave);
                    _previewLights[lightIndex] = lightObject.AddComponent<Light>();
                    _previewLights[lightIndex].type = LightType.Directional;
                }

                SetPreviewLightsActive(false);
            }

            return _previewLights;
        }

        private void SetPreviewLightsActive(bool active)
        {
            Light[] lights = GetPreviewLights();
            foreach (var light in lights) { light.gameObject.SetActive(active); }
        }

        private void SetupPreviewLights()
        {
            Light[] lights = GetPreviewLights();
            lights[0].transform.forward = GetRenderCamera().transform.forward;

            float lightIntensity = 0.5f;
            foreach (var light in lights) light.intensity = lightIntensity;
        }
    }
}
#endif