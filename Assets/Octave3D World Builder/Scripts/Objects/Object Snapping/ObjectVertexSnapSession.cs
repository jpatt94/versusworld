#if UNITY_EDITOR
using UnityEngine;

namespace O3DWB
{
    public class ObjectVertexSnapSession
    {
        #region Private Variables
        private GameObject _sourceObject;
        private GameObject _destinationObject;
        private XZGridCell _destinationGridCell;

        private Vector3 _sourceVertex;
        private Vector3 _snapPosition;

        private ObjectVertexSnapSessionState _state;
        private bool _isActive;

        private ObjectMask _objectMask = new ObjectMask();
        #endregion

        #region Public Properties
        public bool IsActive { get { return _isActive; } }
        public GameObject SourceGameObject { get { return _sourceObject; } }
        public GameObject DestinationGameObject { get { return _destinationObject; } }
        public XZGridCell DestinationGridCell { get { return _destinationGridCell != null ? new XZGridCell(_destinationGridCell) : null; } }
        public Vector3 SourceVertex { get { return _sourceVertex; } }
        public Vector3 SnapPosition { get { return _snapPosition; } }
        public ObjectVertexSnapSessionState State { get { return _state; } }
        #endregion

        #region Public Methods
        public void Begin()
        {
            if (_isActive) return;

            _isActive = true;
            ResetData();

            _state = ObjectVertexSnapSessionState.SelectSourceVertex;
        }

        public void End()
        {
            _isActive = false;
            ResetData();
        }

        public void UpdateForMouseMovement()
        {
            if (!_isActive) return;

            if (MouseButtonStates.Instance.IsMouseButtonDown(MouseButton.Left)) _state = ObjectVertexSnapSessionState.SnapToDestination;
            else _state = ObjectVertexSnapSessionState.SelectSourceVertex;

            if (_state == ObjectVertexSnapSessionState.SelectSourceVertex)
            {
                _objectMask.ObjectCollectionMask.UnmaskAll();
                MouseCursorRayHit cursorRayHit = GetCursorRayHit();
                if (cursorRayHit.WasAnObjectHit)
                {
                    GameObjectRayHit objectRayHit = cursorRayHit.ClosestObjectRayHit;
                    MeshRayHit meshRayHit = objectRayHit.ObjectMeshHit;
                    if (meshRayHit != null)
                    {
                        Octave3DMesh octaveMesh = meshRayHit.HitMesh;

                        Triangle3D sourceTriangle = octaveMesh.GetTriangle(meshRayHit.HitTriangleIndex);
                        sourceTriangle.TransformPoints(objectRayHit.HitObject.transform.localToWorldMatrix);

                        _sourceVertex = sourceTriangle.GetPointClosestToPoint(meshRayHit.HitPoint);
                        _sourceObject = objectRayHit.HitObject;
                        _objectMask.ObjectCollectionMask.Mask(_sourceObject);
                    }
                    else
                    {
                        SpriteRenderer spriteRenderer = objectRayHit.HitObject.GetComponent<SpriteRenderer>();
                        if(spriteRenderer != null)
                        {
                            _sourceObject = objectRayHit.HitObject;
                            _sourceVertex = Vector3Extensions.GetClosestPointToPoint(objectRayHit.ObjectBoxHit.HitBox.GetCenterAndCornerPoints(), objectRayHit.HitPoint);
                            _objectMask.ObjectCollectionMask.Mask(_sourceObject);
                        }
                    }
                }
            }
            else
            {
                MouseCursorRayHit cursorRayHit = GetCursorRayHit();
                if (cursorRayHit.WasAnythingHit)
                {
                    bool useGridCellHit = false;
                    if (!cursorRayHit.WasAnObjectHit) useGridCellHit = true;
                    else
                    if (cursorRayHit.WasAnObjectHit && cursorRayHit.WasACellHit)
                    {
                        if (cursorRayHit.ClosestObjectRayHit.HitObject == _sourceObject || 
                            cursorRayHit.ClosestObjectRayHit.HitObject.transform.IsChildOf(_sourceObject.transform)) useGridCellHit = true;
                        else
                        {
                            float gridCellHitEnter = cursorRayHit.GridCellRayHit.HitEnter;
                            float objectHitEnter = cursorRayHit.ClosestObjectRayHit.HitEnter;
                            if (gridCellHitEnter < Mathf.Max(0.0f, (objectHitEnter - 1e-3f))) useGridCellHit = true;
                        }
                    }

                    if (useGridCellHit)
                    {
                        XZGridCell hitCell = cursorRayHit.GridCellRayHit.HitCell;
                        XZOrientedQuad3D cellQuad = hitCell.Quad;

                        _destinationObject = null;
                        _destinationGridCell = hitCell;

                        _snapPosition = cellQuad.GetPointClosestToPoint(cursorRayHit.GridCellRayHit.HitPoint, true);
                        Snap();
                    }
                    else
                    {
                        GameObjectRayHit objectRayHit = cursorRayHit.ClosestObjectRayHit;
                        MeshRayHit meshRayHit = objectRayHit.ObjectMeshHit;
                        if(meshRayHit != null)
                        {
                            _destinationObject = objectRayHit.HitObject;
                            Triangle3D destinationTriangle = meshRayHit.HitMesh.GetTriangle(meshRayHit.HitTriangleIndex);
                            destinationTriangle.TransformPoints(_destinationObject.transform.localToWorldMatrix);
                            _destinationGridCell = null;

                            _snapPosition = destinationTriangle.GetPointClosestToPoint(meshRayHit.HitPoint);
                            Snap();
                        }
                        else
                        {
                            SpriteRenderer spriteRenderer = objectRayHit.HitObject.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null)
                            {
                                _destinationGridCell = null;
                                _destinationObject = objectRayHit.HitObject;

                                _snapPosition = Vector3Extensions.GetClosestPointToPoint(objectRayHit.ObjectBoxHit.HitBox.GetCenterAndCornerPoints(), objectRayHit.HitPoint);
                                Snap();
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        private void ResetData()
        {
            _sourceObject = null;

            _destinationObject = null;
            _destinationGridCell = null;

            _objectMask.ObjectCollectionMask.UnmaskAll();
        }

        private MouseCursorRayHit GetCursorRayHit()
        {
            MouseCursor.Instance.PushObjectMask(_objectMask);
            MouseCursor.Instance.PushObjectPickMaskFlags(MouseCursorObjectPickFlags.ObjectBox | MouseCursorObjectPickFlags.ObjectTerrain);
            MouseCursorRayHit cursorRayHit = MouseCursor.Instance.GetRayHit();
            MouseCursor.Instance.PopObjectPickMaskFlags();
            MouseCursor.Instance.PopObjectMask();

            return cursorRayHit;
        }

        private void Snap()
        {
            Vector3 snapVector = _snapPosition - _sourceVertex;

            GameObject root = Octave3DWorldBuilder.ActiveInstance.GetRoot(_sourceObject);
            if(root != null)
            {
                Transform parentTransform = root.transform;
                UndoEx.RecordForToolAction(parentTransform);
                parentTransform.position += snapVector;
                _sourceVertex += snapVector;
            }
        }
        #endregion
    }
}
#endif