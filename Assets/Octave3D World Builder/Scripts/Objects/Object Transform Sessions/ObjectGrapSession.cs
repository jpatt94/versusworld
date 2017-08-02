#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace O3DWB
{
    public class ObjectGrapSession
    {
        private enum State
        {
            Inactive = 0,
            Grabbing
        }

        private State _state;
        private List<GameObject> _grabbedObjects;
        private Dictionary<GameObject, Vector3> _objectToPivotDir = new Dictionary<GameObject, Vector3>();
        private ObjectMask _rayHitMask = new ObjectMask();
        private MouseCursorRayHit _currentCursorRayHit;
        private Vector3 _surfaceHitPoint;
        private ObjectGrabSettings _grabSettings;

        public bool IsActive { get { return _state != State.Inactive; } }
        public ObjectGrabSettings Settings { set { if (value != null) _grabSettings = value; } }

        public void Begin(List<GameObject> grabbedObjects)
        {
            if (_grabSettings == null || grabbedObjects == null || grabbedObjects.Count == 0 || IsActive) return;

            _grabbedObjects = new List<GameObject>(grabbedObjects);
            MouseCursorRayHit cursorRayHit = GetCursorRayHit();
            if (!cursorRayHit.WasAnythingHit) return;

            _surfaceHitPoint = cursorRayHit.WasAnObjectHit ? cursorRayHit.ClosestObjectRayHit.HitPoint : cursorRayHit.GridCellRayHit.HitPoint;

            _state = State.Grabbing;
            _rayHitMask.ObjectCollectionMask.Mask(_grabbedObjects);

            foreach(var grabbedObject in grabbedObjects)
            {
                if(grabbedObject != null)
                {
                    _objectToPivotDir.Add(grabbedObject, grabbedObject.transform.position - _surfaceHitPoint);
                }
            }
        }

        public void End()
        {
            _state = State.Inactive;
            if (_grabbedObjects != null) _grabbedObjects.Clear();
            _rayHitMask.ObjectCollectionMask.UnmaskAll();
            _objectToPivotDir.Clear();
        }

        public void RenderGizmos()
        {
            if(IsActive && _grabSettings.ShowGrabLines)
            {
                foreach(var grabbedObject in _grabbedObjects)
                {
                    GizmosEx.RenderLine(grabbedObject.GetHierarchyWorldOrientedBox().Center, _surfaceHitPoint, _grabSettings.GrabLineColor);
                }
            }
        }

        public void Update()
        {
            if(IsActive)
            {
                _currentCursorRayHit = GetCursorRayHit();
                if (!_currentCursorRayHit.WasAnythingHit) return;

                if (_currentCursorRayHit.WasAnObjectHit)
                {
                    GameObjectExtensions.RecordObjectTransformsForUndo(_grabbedObjects);
                    GameObjectRayHit objectRayHit = _currentCursorRayHit.ClosestObjectRayHit;
                    _surfaceHitPoint = objectRayHit.HitPoint;
                    foreach(var grabbedObject in _grabbedObjects)
                    {
                        if (grabbedObject == null) continue;

                        Transform objectTransform = grabbedObject.transform;
                        objectTransform.position = objectRayHit.HitPoint + _objectToPivotDir[grabbedObject];                        
                        if(objectRayHit.WasTerrainHit)
                        {
                            Ray ray = new Ray(grabbedObject.GetHierarchyWorldOrientedBox().Center + Vector3.up, -Vector3.up);
                            GameObjectRayHit sitPointHit = null;
                            if (objectRayHit.HitObject.RaycastTerrainReverseIfFail(ray, out sitPointHit))
                            {
                                if (_grabSettings.AlignAxis) AxisAlignment.AlignObjectAxis(grabbedObject, _grabSettings.AlignmentAxis, sitPointHit.HitNormal);
                                grabbedObject.PlaceHierarchyOnPlane(new Plane(sitPointHit.HitNormal, sitPointHit.HitPoint));
                                if (!_grabSettings.EmbedInSurfaceWhenNoAlign || _grabSettings.AlignAxis) objectTransform.position += _grabSettings.OffsetFromSurface * sitPointHit.HitNormal;
                                if (_grabSettings.EmbedInSurfaceWhenNoAlign && !_grabSettings.AlignAxis) grabbedObject.EmbedInSurfaceByVertex(-Vector3.up, objectRayHit.HitObject);
                            }
                        }
                        else
                        if(objectRayHit.WasMeshHit)
                        {
                            Ray ray = new Ray(grabbedObject.GetHierarchyWorldOrientedBox().Center + objectRayHit.HitNormal * 2.0f, -objectRayHit.HitNormal);
                            GameObjectRayHit sitPointHit = null;
                            if (objectRayHit.HitObject.RaycastMeshReverseIfFail(ray, out sitPointHit))
                            {
                                if (_grabSettings.AlignAxis) AxisAlignment.AlignObjectAxis(grabbedObject, _grabSettings.AlignmentAxis, sitPointHit.HitNormal);
                                grabbedObject.PlaceHierarchyOnPlane(new Plane(sitPointHit.HitNormal, sitPointHit.HitPoint));
                                if (!_grabSettings.EmbedInSurfaceWhenNoAlign || _grabSettings.AlignAxis) objectTransform.position += _grabSettings.OffsetFromSurface * sitPointHit.HitNormal;
                                if (_grabSettings.EmbedInSurfaceWhenNoAlign && !_grabSettings.AlignAxis) grabbedObject.EmbedInSurfaceByVertex(-sitPointHit.HitNormal, objectRayHit.HitObject);
                            }
                        }
                    }
                }
                else
                if (_currentCursorRayHit.WasACellHit)
                {
                    GameObjectExtensions.RecordObjectTransformsForUndo(_grabbedObjects);
                    GridCellRayHit cellRayHit = _currentCursorRayHit.GridCellRayHit;
                    _surfaceHitPoint = cellRayHit.HitPoint;

                    foreach (var grabbedObject in _grabbedObjects)
                    {
                        if (grabbedObject == null) continue;

                        Transform objectTransform = grabbedObject.transform;
                        objectTransform.position = cellRayHit.HitPoint + _objectToPivotDir[grabbedObject];

                        if (_grabSettings.AlignAxis) AxisAlignment.AlignObjectAxis(grabbedObject, _grabSettings.AlignmentAxis, cellRayHit.HitNormal);
                        grabbedObject.PlaceHierarchyOnPlane(new Plane(cellRayHit.HitNormal, cellRayHit.HitPoint));
                        if (!_grabSettings.EmbedInSurfaceWhenNoAlign || _grabSettings.AlignAxis) objectTransform.position += _grabSettings.OffsetFromSurface * cellRayHit.HitNormal;
                    }
                }
            }
        }

        private MouseCursorRayHit GetCursorRayHit()
        {
            MouseCursor.Instance.PushObjectPickMaskFlags(MouseCursorObjectPickFlags.ObjectBox | MouseCursorObjectPickFlags.ObjectSprite);
            MouseCursor.Instance.PushObjectMask(_rayHitMask);
            MouseCursorRayHit cursorRayHit = MouseCursor.Instance.GetRayHit();
            MouseCursor.Instance.PopObjectPickMaskFlags();
            MouseCursor.Instance.PopObjectMask();

            return cursorRayHit;
        }
    }
}
#endif