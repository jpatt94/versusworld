#if UNITY_EDITOR
using UnityEngine;

using UnityEditor;

using System.Reflection;


public class FogVolumeCreator : Editor
{
    [MenuItem("GameObject/Create Other/Fog Volume")]
    [MenuItem("Fog Volume/Create/Fog Volume")]
    public static void CreateFogVolume()
    {
        var FogVolume = new GameObject();

        //Icon stuff
        var Icon = Resources.Load("FogVolumeIcon") as Texture;

        //Icon.hideFlags = HideFlags.HideAndDontSave;
        var editorGUI = typeof(EditorGUIUtility);
        var bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
        var args = new object[]
        {
            FogVolume,
            Icon
        };
        editorGUI.InvokeMember("SetIconForObject", bindingFlags, null, null, args);

        FogVolume.name = "Fog Volume";
        FogVolume.AddComponent<MeshRenderer>();
        FogVolume.AddComponent<FogVolume>();
        FogVolume.GetComponent<Renderer>().shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
        FogVolume.GetComponent<Renderer>().receiveShadows = false;
        FogVolume.GetComponent<Renderer>().reflectionProbeUsage =
                UnityEngine.Rendering.ReflectionProbeUsage.Off;
        FogVolume.GetComponent<Renderer>().lightProbeUsage =
                UnityEngine.Rendering.LightProbeUsage.Off;
        Selection.activeObject = FogVolume;
        if (SceneView.currentDrawingSceneView)
        {
            SceneView.currentDrawingSceneView.MoveToView(FogVolume.transform);
        }
    }

    [MenuItem("GameObject/Create Other/Fog Volume Point Light")]
    [MenuItem("Fog Volume/Create/Fog Volume Point Light")]
    public static void CreateFogVolumePointLight()
    {
        var fogVolumeLight = new GameObject("FogVolumePointLight");
        var light = fogVolumeLight.AddComponent<FogVolumeLight>();
        light.IsPointLight = true;
        light.IsAddedToNormalLight = true;
    }

    [MenuItem("GameObject/Create Other/Fog Volume Spot Light")]
    [MenuItem("Fog Volume/Create/Fog Volume Spot Light")]
    public static void CreateFogVolumeSpotLight()
    {
        var fogVolumeLight = new GameObject("FogVolumeSpotLight");
        var light = fogVolumeLight.AddComponent<FogVolumeLight>();
        light.IsPointLight = false;
        light.IsAddedToNormalLight = true;
    }

    [MenuItem("Fog Volume/Create/Fog Volume Directional Light")]
    public static void AutoCreateFogVolumeDirectionalLight()
    {
        var lights = FindObjectsOfType<Light>();
        for (var i = 0; i < lights.Length; i++)
        {
            var light = lights[i];
            if (light.type == LightType.Directional)
            {
                if (light.GetComponent<FogVolumeDirectionalLight>() == null)
                {
                    var dirLight = light.gameObject.AddComponent<FogVolumeDirectionalLight>();
                    var fogVolumes = FindObjectsOfType<FogVolume>();
                    dirLight._TargetFogVolumes = new FogVolume[fogVolumes.Length];
                    for (var k = 0; k < fogVolumes.Length; k++)
                    {
                        dirLight._TargetFogVolumes[k] = fogVolumes[k];
                    }
                }
            }
        }
    }

    [MenuItem("Fog Volume/Create/Fog Volume Directional Light", true)]
    public static bool EnableCreateFogVolumeDirectionalLight()
    {
        return FindObjectOfType<FogVolumeDirectionalLight>() == null &&
               FindObjectOfType<FogVolume>() != null;
    }
    [MenuItem("GameObject/Create Other/Normal Point Light")]
    [MenuItem("Fog Volume/Create/Normal Point Light")]
    public static void CreateNormalPointLight()
    {
        var normalPointLight = new GameObject("Point light");
        var light = normalPointLight.AddComponent<Light>();
        light.type = LightType.Point;
        var fvLight = normalPointLight.AddComponent<FogVolumeLight>();
        fvLight.IsPointLight = true;
        fvLight.IsAddedToNormalLight = true;
    }

    [MenuItem("GameObject/Create Other/Normal Spot Light")]
    [MenuItem("Fog Volume/Create/Normal Spot Light")]
    public static void CreateNormalSpotLight()
    {
        var normalPointLight = new GameObject("Spot light");
        var light = normalPointLight.AddComponent<Light>();
        light.type = LightType.Spot;
        var fvLight = normalPointLight.AddComponent<FogVolumeLight>();
        fvLight.IsPointLight = false;
        fvLight.IsAddedToNormalLight = true;
    }
}
#endif
