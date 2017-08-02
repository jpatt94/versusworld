#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace O3DWB
{
    public static class EditorGUIStylesFactory
    {
        #region Public Static Functions
        public static GUIStyle CreateInformativeLabelStyle(Color color, bool requireWordWrap = true)
        {
            var style = new GUIStyle();
            style.wordWrap = requireWordWrap;
            style.normal.textColor = color;

            return style;
        }
        #endregion
    }
}
#endif