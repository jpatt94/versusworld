#if UNITY_EDITOR
using UnityEngine;
using System;

namespace O3DWB
{
    [Serializable]
    public class ObjectSelectionActionsViewData : ScriptableObject
    {
        #region Private Variables
        [SerializeField]
        private int _selectionAssignmentLayer = LayerExtensions.GetDefaultLayer();
        #endregion

        #region Public Properties
        public int SelectionAssignmentLayer { get { return _selectionAssignmentLayer; } set { if (LayerExtensions.IsLayerNumberValid(value)) _selectionAssignmentLayer = value; } }
        #endregion
    }
}
#endif