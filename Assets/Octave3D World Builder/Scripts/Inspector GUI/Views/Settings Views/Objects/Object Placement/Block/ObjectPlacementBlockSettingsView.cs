#if UNITY_EDITOR
using UnityEngine;
using System;

namespace O3DWB
{
    [Serializable]
    public class ObjectPlacementBlockSettingsView : SettingsView
    {
        #region Private Variables
        [NonSerialized]
        private ObjectPlacementBlockSettings _settings;
        #endregion

        #region Constructors
        public ObjectPlacementBlockSettingsView(ObjectPlacementBlockSettings settings)
        {
            _settings = settings;
        }
        #endregion

        #region Protected Methods
        protected override void RenderContent()
        {
            _settings.ManualConstructionSettings.View.Render();
        }
        #endregion
    }
}
#endif