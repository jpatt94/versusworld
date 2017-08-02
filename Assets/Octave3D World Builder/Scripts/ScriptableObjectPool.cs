#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace O3DWB
{
    [ExecuteInEditMode]
    public class ScriptableObjectPool : MonoBehaviour
    {
        #region Private Variables
        [SerializeField]
        private List<ScriptableObject> _scriptableObjects = new List<ScriptableObject>();
        #endregion

        #region Public Properties
        public int Count { get { return _scriptableObjects.Count; } }
        #endregion

        #region Public Methods
        public T CreateScriptableObject<T>() where T : ScriptableObject
        {
            UndoEx.RecordForToolAction(this);
            T scriptableObject = ScriptableObject.CreateInstance<T>();
            if (scriptableObject != null)
            {
                UndoEx.RegisterCreatedScriptableObject(scriptableObject);
                _scriptableObjects.Add(scriptableObject);
            }

            return scriptableObject;
        }

        public void DestroyAllScriptableObjects()
        {
            RemoveNullEntries();
            var copy = new List<ScriptableObject>(_scriptableObjects);  // Store copy because destroying some scriptable objects can cause the destruction and
                                                                        // removal of others so iterating thorugh the original collection can throw exceptions.
            foreach (ScriptableObject scriptableObject in copy)
            {
                // Note: Still need to check for null in case the destruction of previous scriptable
                //       objects caused others to be destroyed.
                if (scriptableObject != null) DestroyImmediate(scriptableObject);
            }
            _scriptableObjects.Clear();
        }

        public void RemoveNullEntries()
        {
            _scriptableObjects.RemoveAll(item => item == null);
        }
        #endregion

        #region Private Methods
        private void OnDestroy()
        {
            DestroyAllScriptableObjects();
        }
        #endregion
    }
}
#endif