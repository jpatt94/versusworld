#if UNITY_EDITOR
using UnityEngine;
using System;

namespace O3DWB
{
    [Serializable]
    public class ObjectSelectionSettings : ScriptableObject
    {
        #region Private Variables
        [SerializeField]
        private ObjectSelectionShapeType _selectionShapeType = ObjectSelectionShapeType.Rectangle;
        [SerializeField]
        private ObjectSelectionMode _selectionMode = ObjectSelectionMode.Standard;
        [SerializeField]
        private ObjectSelectionUpdateMode _selectionUpdateMode = ObjectSelectionUpdateMode.EntireHierarchy;

        [SerializeField]
        private bool _allowPartialOverlap = true;
        [SerializeField]
        private ObjectSelectionPaintModeSettings _paintModeSettings;

        [SerializeField]
        private bool _attachMirroredObjectsToActiveObjectGroup = false;

        [SerializeField]
        private ObjectSelectionSettingsView _view;
        #endregion

        #region Public Properties
        public ObjectSelectionShapeType SelectionShapeType { get { return _selectionShapeType; } set { _selectionShapeType = value; } }
        public ObjectSelectionMode SelectionMode { get { return _selectionMode; } set { _selectionMode = value; } }
        public ObjectSelectionUpdateMode SelectionUpdateMode { get { return _selectionUpdateMode; } set { _selectionUpdateMode = value; } }
        public bool AllowPartialOverlap { get { return _allowPartialOverlap; } set { _allowPartialOverlap = value; } }
        public ObjectSelectionPaintModeSettings PaintModeSettings
        {
            get
            {
                if (_paintModeSettings == null) _paintModeSettings = Octave3DWorldBuilder.ActiveInstance.CreateScriptableObject<ObjectSelectionPaintModeSettings>();
                return _paintModeSettings;
            }
        }
        public bool AttachMirroredObjectsToActiveObjectGroup { get { return _attachMirroredObjectsToActiveObjectGroup; } set { _attachMirroredObjectsToActiveObjectGroup = value; } }
        public ObjectSelectionSettingsView View { get { return _view; } }
        #endregion

        #region Constructors
        public ObjectSelectionSettings()
        {
            _view = new ObjectSelectionSettingsView(this);
        }
        #endregion

        #region Public Static Functions
        public static ObjectSelectionSettings Get()
        {
            return ObjectSelection.Get().Settings;
        }
        #endregion
    }
}
#endif