#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace O3DWB
{
    [Serializable]
    public class ObjectSelectionSettingsView : SettingsView
    {
        #region Private Variables
        [NonSerialized]
        private ObjectSelectionSettings _settings;
        #endregion

        #region Constructors
        public ObjectSelectionSettingsView(ObjectSelectionSettings settings)
        {
            _settings = settings;

            ToggleVisibilityBeforeRender = true;
            VisibilityToggleLabel = "Object Selection Settings";
            SurroundWithBox = true;
        }
        #endregion

        #region Protected Methods
        protected override void RenderContent()
        {
            RenderAllowPartialOverlapToggle();
            RenderAttachMirroredObjectsToActiveObjectGroupToggle();

            EditorGUILayout.Separator();
            RenderSelectionShapeTypeSelectionPopup();
            RenderSelectionUpdateModeSelectionPopup();
            RenderSelectionModeSelectionPopup();

            if (_settings.SelectionMode == ObjectSelectionMode.Paint) _settings.PaintModeSettings.View.Render();
        }
        #endregion

        #region Private Methods
        private void RenderAllowPartialOverlapToggle()
        {
            bool newBool = EditorGUILayout.ToggleLeft(GetContentForAllowPartialOverlapToggle(), _settings.AllowPartialOverlap);
            if (newBool != _settings.AllowPartialOverlap)
            {
                UndoEx.RecordForToolAction(_settings);
                _settings.AllowPartialOverlap = newBool;
            }
        }

        private GUIContent GetContentForAllowPartialOverlapToggle()
        {
            var content = new GUIContent();
            content.text = "Allow partial overlap";
            content.tooltip = "When this is NOT checked, objects will be selected ONLY if their screen rectangle is totally contained by the selection shape.";

            return content;
        }

        private void RenderSelectionShapeTypeSelectionPopup()
        {
            ObjectSelectionShapeType newSelectionShapeType = (ObjectSelectionShapeType)EditorGUILayout.EnumPopup(GetContentForSelectionShapeTypeSelectionPopup(), _settings.SelectionShapeType);
            if (newSelectionShapeType != _settings.SelectionShapeType)
            {
                UndoEx.RecordForToolAction(_settings);
                _settings.SelectionShapeType = newSelectionShapeType;
            }
        }

        private GUIContent GetContentForSelectionShapeTypeSelectionPopup()
        {
            var content = new GUIContent();
            content.text = "Shape type";
            content.tooltip = "Allows you to choose the type of shape which is used to select objects in the scene.";

            return content;
        }

        private void RenderSelectionUpdateModeSelectionPopup()
        {
            ObjectSelectionUpdateMode newSelectionUpdateMode = (ObjectSelectionUpdateMode)EditorGUILayout.EnumPopup(GetCotentForSelectionUpdateModeSelectionToggle(), _settings.SelectionUpdateMode);
            if (newSelectionUpdateMode != _settings.SelectionUpdateMode)
            {
                UndoEx.RecordForToolAction(_settings);
                _settings.SelectionUpdateMode = newSelectionUpdateMode;
            }
        }

        private GUIContent GetCotentForSelectionUpdateModeSelectionToggle()
        {
            var content = new GUIContent();
            content.text = "Selection update mode";
            content.tooltip = "Allows you to control the way in which the object selection is updated.";

            return content;
        }

        private void RenderSelectionModeSelectionPopup()
        {
            ObjectSelectionMode newSelectionMode = (ObjectSelectionMode)EditorGUILayout.EnumPopup(GetContentForSelectionModeSelectionPopup(), _settings.SelectionMode);
            if (newSelectionMode != _settings.SelectionMode)
            {
                UndoEx.RecordForToolAction(_settings);
                _settings.SelectionMode = newSelectionMode;
            }
        }

        private GUIContent GetContentForSelectionModeSelectionPopup()
        {
            var content = new GUIContent();
            content.text = "Selection mode";
            content.tooltip = "Allows you to control the way in which object selection is performed.";

            return content;
        }

        private void RenderAttachMirroredObjectsToActiveObjectGroupToggle()
        {
            bool newBool = EditorGUILayout.ToggleLeft(GetContentForAttachMirroredObjectsToActiveObjectGroupToggle(), _settings.AttachMirroredObjectsToActiveObjectGroup);
            if(newBool != _settings.AttachMirroredObjectsToActiveObjectGroup)
            {
                UndoEx.RecordForToolAction(_settings);
                _settings.AttachMirroredObjectsToActiveObjectGroup = newBool;
            }
        }

        private GUIContent GetContentForAttachMirroredObjectsToActiveObjectGroupToggle()
        {
            var content = new GUIContent();
            content.text = "Attach mirrored objects to active group";
            content.tooltip = "If this is checked, when selected objects are mirrored, they will be attached to the currently active object group (if any).";

            return content;
        }
        #endregion
    }
}
#endif