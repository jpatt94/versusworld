#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

namespace O3DWB
{
    [Serializable]
    public class ObjectSelectionTransformGizmoSystem : ScriptableObject
    {
        #region Private Variables
        [SerializeField]
        private bool _areGizmosActive = true;

        [SerializeField]
        private TransformGizmoType _activeGizmoType = TransformGizmoType.Move;
        [SerializeField]
        private TransformSpace _gizmoTransformSpace = TransformSpace.Global;
        [SerializeField]
        private TransformGizmoPivotPoint _gizmoTransformPivotPoint = TransformGizmoPivotPoint.Center;

        [SerializeField]
        private ObjectMoveGizmo _objectMoveGizmo;
        [SerializeField]
        private ObjectRotationGizmo _objectRotationGizmo;
        [SerializeField]
        private ObjectScaleGizmo _objectScaleGizmo;
        #endregion

        #region Public Properties
        public bool AreGizmosActive
        {
            get { return _areGizmosActive; }
            set
            {
                _areGizmosActive = value;
                SceneView.RepaintAll();
            }
        }

        public TransformGizmoType ActiveGizmoType 
        {
            get { return _activeGizmoType; }
            set
            { 
                _activeGizmoType = value;
                AdjustActiveGizmoRotation();
                AdjustActiveGizmoPosition();

                SceneView.RepaintAll();
            } 
        }
        public TransformSpace GizmoTransformSpace 
        { 
            get { return _gizmoTransformSpace; }
            set 
            { 
                _gizmoTransformSpace = value;
                AdjustActiveGizmoRotation();

                SceneView.RepaintAll();
            } 
        }
        public TransformGizmoPivotPoint GizmoTransformPivotPoint 
        { 
            get { return _gizmoTransformPivotPoint; } 
            set 
            { 
                _gizmoTransformPivotPoint = value;
                AdjustActiveGizmoPosition();

                SceneView.RepaintAll();
            } 
        }
        public ObjectMoveGizmo ObjectMoveGizmo
        {
            get
            {
                if (_objectMoveGizmo == null) _objectMoveGizmo = Octave3DWorldBuilder.ActiveInstance.CreateScriptableObject<ObjectMoveGizmo>();
                return _objectMoveGizmo;
            }
        }
        public ObjectRotationGizmo ObjectRotationGizmo
        {
            get
            {
                if (_objectRotationGizmo == null) _objectRotationGizmo = Octave3DWorldBuilder.ActiveInstance.CreateScriptableObject<ObjectRotationGizmo>();
                return _objectRotationGizmo;
            }
        }
        public ObjectScaleGizmo ObjectScaleGizmo
        {
            get
            {
                if (_objectScaleGizmo == null) _objectScaleGizmo = Octave3DWorldBuilder.ActiveInstance.CreateScriptableObject<ObjectScaleGizmo>();
                return _objectScaleGizmo;
            }
        }
        #endregion

        #region Public Methods
        public bool OwnsGizmo(ObjectTransformGizmo objectTransformGizmo)
        {
            if (objectTransformGizmo == ObjectMoveGizmo) return true;
            if (objectTransformGizmo == ObjectRotationGizmo) return true;
            if (objectTransformGizmo == ObjectScaleGizmo) return true;
            return false;
        }

        public void RenderHandles(HashSet<GameObject> selectedGameObjects)
        {
            if (selectedGameObjects == null || selectedGameObjects.Count == 0 || !_areGizmosActive) return;

            ObjectTransformGizmo activeGizmo = GetActiveGizmo();
            if (activeGizmo != null)
            {
                activeGizmo.GameObjectsWhichCanBeTransformed = selectedGameObjects;
                activeGizmo.RenderHandles(_gizmoTransformPivotPoint);
            }
        }

        public void OnObjectSelectionUpdatedUsingMouseClick()
        {
            ObjectTransformGizmo activeGizmo = GetActiveGizmo();
            if (activeGizmo != null)
            {
                GameObject lastSelectedGameObject = ObjectSelection.Get().GetLastSelectedGameObject();
                if (lastSelectedGameObject == null) return;
                Transform lastSelectedObjectTransform = lastSelectedGameObject.transform;

                if (_gizmoTransformPivotPoint == TransformGizmoPivotPoint.Pivot && lastSelectedGameObject != null) activeGizmo.WorldPosition = lastSelectedObjectTransform.position;
                else activeGizmo.WorldPosition = ObjectSelection.Get().GetWorldCenter();

                if (_gizmoTransformSpace == TransformSpace.Local && lastSelectedGameObject != null) activeGizmo.WorldRotation = lastSelectedObjectTransform.rotation;
                else activeGizmo.WorldRotation = Quaternion.identity;
            }
        }

        public void OnObjectSelectionUpdatedUsingSelectionShape()
        {
            ObjectTransformGizmo activeGizmo = GetActiveGizmo();
            if (activeGizmo != null)
            {
                GameObject firstSelectedGameObject = ObjectSelection.Get().GetFirstSelectedGameObject();
                if (firstSelectedGameObject == null) return;
                Transform firstSelectedObjectTransform = firstSelectedGameObject.transform;

                if (_gizmoTransformPivotPoint == TransformGizmoPivotPoint.Pivot && firstSelectedGameObject != null) activeGizmo.WorldPosition = firstSelectedObjectTransform.position;           
                else activeGizmo.WorldPosition = ObjectSelection.Get().GetWorldCenter();

                if (_gizmoTransformSpace == TransformSpace.Local && firstSelectedObjectTransform != null) activeGizmo.WorldRotation = firstSelectedObjectTransform.rotation;
                else activeGizmo.WorldRotation = Quaternion.identity;
            }
        }

        public void OnObjectSelectionUpdated()
        {
            OnObjectSelectionUpdatedUsingSelectionShape();
        }
        #endregion

        #region Private Methods
        private ObjectTransformGizmo GetActiveGizmo()
        {
            if (_activeGizmoType == TransformGizmoType.Move) return ObjectMoveGizmo;
            if (_activeGizmoType == TransformGizmoType.Rotate) return ObjectRotationGizmo;
            if (_activeGizmoType == TransformGizmoType.Scale) return ObjectScaleGizmo;
            return null;
        }

        private void AdjustActiveGizmoPosition()
        {
            ObjectTransformGizmo activeGizmo = GetActiveGizmo();
            if (activeGizmo != null)
            {
                if (_gizmoTransformPivotPoint == TransformGizmoPivotPoint.Pivot)
                {
                    GameObject firstSelectedObject = ObjectSelection.Get().GetFirstSelectedGameObject();
                    if (firstSelectedObject != null) activeGizmo.WorldPosition = firstSelectedObject.transform.position;
                }
                else activeGizmo.WorldPosition = ObjectSelection.Get().GetWorldCenter();
            }
        }

        private void AdjustActiveGizmoRotation()
        {
            ObjectTransformGizmo activeGizmo = GetActiveGizmo();
            if (activeGizmo != null)
            {
                if (_gizmoTransformSpace == TransformSpace.Local)
                {
                    GameObject firstSelectedObject = ObjectSelection.Get().GetFirstSelectedGameObject();
                    if (firstSelectedObject != null) activeGizmo.WorldRotation = firstSelectedObject.transform.rotation;
                }
                else activeGizmo.WorldRotation = Quaternion.identity;
            }
        }
        #endregion
    }
}
#endif