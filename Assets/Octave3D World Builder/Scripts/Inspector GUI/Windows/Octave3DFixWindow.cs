#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace O3DWB
{
    [Serializable]
    public class Octave3DFixWindow : Octave3DEditorWindow
    {
        #region Private Variables
        [SerializeField]
        private Vector2 _scrollViewPosition = Vector2.zero;
        #endregion

        public static Octave3DFixWindow Get()
        {
            return Octave3DWorldBuilder.ActiveInstance.EditorWindowPool.Octave3DFixWindow;
        }

        #region Public Methods
        public override string GetTitle()
        {
            return "Octave3D Fix";
        }

        public override void ShowOctave3DWindow()
        {
            ShowDockable(true);
        }
        #endregion

        #region Protected Methods
        protected override void RenderContent()
        {
            _scrollViewPosition = EditorGUILayout.BeginScrollView(_scrollViewPosition);
            EditorGUILayout.HelpBox("Please press the fix button if you have been using an Octave3D version prior to 2.2. This will cleanup of the scene " +
                                    "of any stray Octave3D internal module objects (this was a bug in previous versions) and also it is required when uprading " + 
                                    "from an earlier version to 2.2 or above.", UnityEditor.MessageType.Info);
            var content = new GUIContent();
            content.text = "Fix";
            content.tooltip = "Applies necessary fixes.";
            if (GUILayout.Button(content, GUILayout.Width(90.0f)))
            {
                var scriptableObjectPools = FindObjectsOfType<ScriptableObjectPool>();
                bool foundScriptablePool = false;
                for(int index = 0; index < scriptableObjectPools.Length; ++index)
                {
                    EditorUtility.DisplayProgressBar("Octave3D Fix", "Cleaning up scriptable object pools...", (float)index / scriptableObjectPools.Length);
                    ScriptableObjectPool pool = scriptableObjectPools[index];
                    if(pool != null && pool.gameObject.GetComponent<Octave3DWorldBuilder>() == null)
                    {
                        GameObject gameObj = pool.gameObject;
                        pool.DestroyAllScriptableObjects();
                        GameObject.DestroyImmediate(pool);
                        GameObject.DestroyImmediate(gameObj);
                        foundScriptablePool = true;
                    }
                }

                var editorWindowPools = FindObjectsOfType<EditorWindowPool>();
                bool foundEditorWindowPool = false;
                for (int index = 0; index < editorWindowPools.Length; ++index)
                {
                    EditorUtility.DisplayProgressBar("Octave3D Fix", "Cleaning up editor window pools...", (float)index / editorWindowPools.Length);
                    EditorWindowPool pool = editorWindowPools[index];
                    if (pool != null && pool.gameObject.GetComponent<Octave3DWorldBuilder>() == null)
                    {
                        GameObject gameObj = pool.gameObject;
                        GameObject.DestroyImmediate(pool);
                        GameObject.DestroyImmediate(gameObj);
                        foundEditorWindowPool = true;
                    }
                }
                EditorUtility.ClearProgressBar();

                if (!foundScriptablePool && !foundEditorWindowPool)
                {
                    EditorUtility.DisplayDialog("All fine!", "There were no stray objects left behind in this scene :)", "OK");
                }
                else EditorUtility.DisplayDialog("Done!", "Cleanup successfully completed!", "OK");
            }
            EditorGUILayout.EndScrollView();
        }
        #endregion
    }
}
#endif