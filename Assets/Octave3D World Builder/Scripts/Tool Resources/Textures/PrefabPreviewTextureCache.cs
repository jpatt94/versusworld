#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace O3DWB
{
    public class PrefabPreviewTextureCache
    {
        #region Private Variables
        private Dictionary<Prefab, Texture2D> _prefabToPreviewTexture = new Dictionary<Prefab, Texture2D>();
        #endregion

        #region Public Static Functions
        public static PrefabPreviewTextureCache Get()
        {
            if (Octave3DWorldBuilder.ActiveInstance == null) return null;
            return Octave3DWorldBuilder.ActiveInstance.ToolResources.PrefabPreviewTextureCache;
        }
        #endregion

        #region Public Methods
        public bool IsPreviewTextureAvailableForPrefab(Prefab prefab)
        {
            return _prefabToPreviewTexture.ContainsKey(prefab);
        }

        public Texture2D GetPrefabPreviewTexture(Prefab prefab)
        {
            if (IsPreviewTextureAvailableForPrefab(prefab)) return _prefabToPreviewTexture[prefab];
            return GeneratePrefabPreviewTextureAndStore(prefab);
        }

        public void DisposeTextures()
        {
            foreach (var pair in _prefabToPreviewTexture)
            {
                if (pair.Value != null) Octave3DWorldBuilder.DestroyImmediate(pair.Value, true);
            }
            _prefabToPreviewTexture.Clear();
        }

        public void DestroyTexturesForNullPrefabEntries()
        {
            Dictionary<Prefab, Texture2D> newPrefabToPreviewTexture = GenerateNewDictionaryExcludingPairsWithNullPrefabReferences();

            _prefabToPreviewTexture.Clear();
            _prefabToPreviewTexture = newPrefabToPreviewTexture;
        }
        #endregion

        #region Private Methods
        private Texture2D GeneratePrefabPreviewTextureAndStore(Prefab prefab)
        {
            // Note: This is needed because in some situations, the 'AssetPreview.GetAssetPreview' function returns null
            //       even when it shouldn't. This happens most often with prefabs that contain meshes created in thrid
            //       party softare.
            EditorUtility.SetDirty(prefab.UnityPrefab);

            Texture2D prefabPreview = AssetPreview.GetAssetPreview(prefab.UnityPrefab);
            return ClonePrefabPreviewAndStore(prefab, prefabPreview);
        }

        private Texture2D ClonePrefabPreviewAndStore(Prefab prefab, Texture2D prefabPreview)
        {
            if (prefabPreview != null)
            {
                Texture2D clonedPreviewTexture = prefabPreview.Clone(true);
                if (clonedPreviewTexture != null)
                {
                    _prefabToPreviewTexture.Add(prefab, clonedPreviewTexture);
                    return clonedPreviewTexture;
                }
            }

            return null;
        }

        private Dictionary<Prefab, Texture2D> GenerateNewDictionaryExcludingPairsWithNullPrefabReferences()
        {
            var newPrefabPreviewTextureDictionary = new Dictionary<Prefab, Texture2D>();
            foreach (KeyValuePair<Prefab, Texture2D> pair in _prefabToPreviewTexture)
            {
                if ((pair.Key == null || pair.Key.UnityPrefab == null) && pair.Value != null) Octave3DWorldBuilder.DestroyImmediate(pair.Value);
                else newPrefabPreviewTextureDictionary.Add(pair.Key, pair.Value);
            }

            return newPrefabPreviewTextureDictionary;
        }
        #endregion
    }
}
#endif