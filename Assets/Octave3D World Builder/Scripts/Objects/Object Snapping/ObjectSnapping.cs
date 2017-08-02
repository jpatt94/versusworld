#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;

namespace O3DWB
{
    [Serializable]
    public class ObjectSnapping : ScriptableObject
    {
        #region Private Variables
        [SerializeField]
        private ObjectSnapSettings _settings;
        [SerializeField]
        private ObjectMask _objectSnapMask = new ObjectMask();
        [SerializeField]
        private XZGrid _xzSnapGrid;
        [SerializeField]
        private SnapSurface _objectSnapSurface = new SnapSurface();

        [SerializeField]
        private bool _wasInitialized = false;
        #endregion
        
        #region Public Properties
        public ObjectMask ObjectSnapMask { get { return _objectSnapMask; } }
        public ObjectSnapSettings Settings
        {
            get
            {
                if (_settings == null) _settings = Octave3DWorldBuilder.ActiveInstance.CreateScriptableObject<ObjectSnapSettings>();
                return _settings;
            }
        }
        public XZGrid XZSnapGrid
        {
            get
            {
                if (_xzSnapGrid == null) _xzSnapGrid = Octave3DWorldBuilder.ActiveInstance.CreateScriptableObject<XZGrid>();
                return _xzSnapGrid;
            }
        }
        public XZGridRenderSettings RenderSettingsForColliderSnapSurfaceGrid { get { return _objectSnapSurface.RenderSettingsForColliderSnapSurfaceGrid; } }
        public Plane ObjectSnapSurfacePlane { get { return _objectSnapSurface.Plane; } }
        public GameObject ObjectSnapSurfaceObject { get { return _objectSnapSurface.SurfaceObject; } }
        public SnapSurfaceType SnapSurfaceType { get { return _objectSnapSurface.SurfaceType; } }
        #endregion

        #region Public Static Functions
        public static ObjectSnapping Get()
        {
            return Octave3DWorldBuilder.ActiveInstance.ObjectSnapping;
        }
        #endregion

        #region Public Methods
        public void RefreshSnapSurface()
        {
            _objectSnapSurface.Refresh();
        }

        public void RenderGizmos()
        {
            XZSnapGrid.RenderGizmos();
            if (!Settings.SnapToCursorHitPoint) _objectSnapSurface.RenderGizmos();
        }

        public void SnapXZGridToCursorPickPoint(bool snapToClosestTopOrBottom)
        {
            MouseCursor.Instance.PushObjectMask(null);
            MouseCursor.Instance.PushObjectPickMaskFlags(MouseCursorObjectPickFlags.ObjectBox | MouseCursorObjectPickFlags.ObjectTerrain);
            MouseCursorRayHit cursorRayHit = MouseCursor.Instance.GetRayHit();
            MouseCursor.Instance.PopObjectPickMaskFlags();
            MouseCursor.Instance.PopObjectMask();

            if (cursorRayHit.WasAnythingHit)
            {
                Vector3 snapDestPoint;
                if (cursorRayHit.WasAnObjectHit)
                {
                    snapDestPoint = cursorRayHit.ClosestObjectRayHit.HitPoint;
                    UndoEx.RecordForToolAction(XZSnapGrid);

                    if(snapToClosestTopOrBottom)
                    {
                        Box objectWorldBox = cursorRayHit.ClosestObjectRayHit.HitObject.GetWorldBox();
                        Vector3 fromCenterToHitPoint = snapDestPoint - objectWorldBox.Center;
                        if (Vector3.Dot(fromCenterToHitPoint, Vector3.up) > 0.0f) snapDestPoint = objectWorldBox.Center + Vector3.up * objectWorldBox.Extents.y;
                        else snapDestPoint = objectWorldBox.Center - Vector3.up * objectWorldBox.Extents.y;
                    }

                    XZSnapGrid.SnapToPoint(snapDestPoint);
                }
            }
        }

        public List<XZGrid> GetAllSnapGrids()
        {
            var allSnapGrids = new List<XZGrid>();
            allSnapGrids.Add(XZSnapGrid);

            return allSnapGrids;
        }

        public void UpdateProjectedBoxFacePivotPoints(GameObject hierarchyRoot, ProjectedBoxFacePivotPoints projectedBoxFacePivotPoints, bool keepCurrentSnapSurface)
        {
            OrientedBox hierarchyWorldOrientedBox = hierarchyRoot.GetHierarchyWorldOrientedBox();
            if (!hierarchyWorldOrientedBox.IsValid()) return;

            if(keepCurrentSnapSurface)
            {
                if (!_objectSnapSurface.IsValid) return;
                projectedBoxFacePivotPoints.FromOrientedBoxAndSnapSurface(hierarchyWorldOrientedBox, _objectSnapSurface);
            }
            else
            {
                _objectSnapSurface.FromMouseCursorRayHit(AcquireCursorRayHit());
                projectedBoxFacePivotPoints.FromOrientedBoxAndSnapSurface(hierarchyWorldOrientedBox, _objectSnapSurface);
            }
        }

        public void SnapObjectHierarchy(GameObject hierarchyRoot, ProjectedBoxFacePivotPoints projectedBoxFacePivotPoints, float offsetFromSnapSurface)
        {
            _objectSnapSurface.FromMouseCursorRayHit(AcquireCursorRayHit());
            if (_objectSnapSurface.IsValid)
            {
                OrientedBox hierarchyWorldOrientedBox = hierarchyRoot.GetHierarchyWorldOrientedBox();
                if (!hierarchyWorldOrientedBox.IsValid()) return;

                Vector3 pivotPoint = projectedBoxFacePivotPoints.ActivePoint;
                if (Settings.UseOriginalPivot) pivotPoint = hierarchyRoot.transform.position;

                if (Settings.SnapToCursorHitPoint || Settings.EnableObjectToObjectSnap)
                {
                    SnapObjectHierarchyPosition(hierarchyRoot, pivotPoint, _objectSnapSurface.CursorPickPoint, projectedBoxFacePivotPoints, offsetFromSnapSurface);
                }
                else
                {
                    if (_objectSnapSurface.SurfaceType == SnapSurfaceType.GridCell && Settings.SnapCenterToCenterForXZGrid && !Settings.UseOriginalPivot) SnapObjectHierarchyToCenterOfSnapSurface(hierarchyRoot, projectedBoxFacePivotPoints.CenterPoint, projectedBoxFacePivotPoints, offsetFromSnapSurface);
                    else
                    if (_objectSnapSurface.SurfaceType == SnapSurfaceType.ObjectCollider && Settings.SnapCenterToCenterForObjectSurface && !Settings.UseOriginalPivot) SnapObjectHierarchyToCenterOfSnapSurface(hierarchyRoot, projectedBoxFacePivotPoints.CenterPoint, projectedBoxFacePivotPoints, offsetFromSnapSurface);
                    else SnapObjectHierarchyPosition(hierarchyRoot, pivotPoint, _objectSnapSurface.GetSnapDestinationPointClosestToCursorPickPoint(), projectedBoxFacePivotPoints, offsetFromSnapSurface);
                    if (AllShortcutCombos.Instance.KeepSnappedHierarchyInSnapSurfaceArea.IsActive()) KeepSnappedHierarchyInSnapSurfaceArea(hierarchyRoot, projectedBoxFacePivotPoints);
                }

                if (Settings.EnableObjectToObjectSnap) SnapHierarchyToNearbyObjects(hierarchyRoot, projectedBoxFacePivotPoints);
            }
        }

        public void SnapHierarchyToNearbyObjects(GameObject hierarchyRoot, ProjectedBoxFacePivotPoints projectedBoxFacePivotPoints)
        {
            Vector3 chosenSnapDestination = Vector3.zero, chosenSnapPivot = Vector3.zero;
            float minDistance = float.MaxValue;
            float snapEpsilon = Settings.ObjectToObjectSnapEpsilon;

            // Snapping will only work if there is at least one mesh object in the hierarchy or at least one sprite renderer
            // with a valid sprite. So we will first get that out of the way. If this condition is not met, we will just return.
            List<GameObject> meshObjectsInHierarchy = hierarchyRoot.GetHierarchyObjectsWithMesh();
            List<GameObject> spriteObjectsInHierarchy = hierarchyRoot.GetHierarchyObjectsWithSprites();
            if (meshObjectsInHierarchy.Count == 0 && spriteObjectsInHierarchy.Count == 0) return;

            // When snapping, we will need to collect the nearby snap destination points (e.g. vertices or box corner points).
            // In order to do this we will perform an overlap test using the hierarchy's world box. The size of the box is
            // increased by 'snapEpsilon' on all axes to account for the snap espilon.
            Box hierarchyWorldBox = hierarchyRoot.GetHierarchyWorldBox();
            if (!hierarchyWorldBox.IsValid()) return;
            Box hierarchyQueryBox = hierarchyWorldBox;
            hierarchyQueryBox.Size = hierarchyQueryBox.Size + Vector3.one * snapEpsilon;

            // Acquire the nearby objects that contain the possible snap destination points
            List<GameObject> nearbyObjects = Octave3DScene.Get().OverlapBox(hierarchyQueryBox);
            if (nearbyObjects.Count == 0) return;

            // If the user chose vertex snapping, we will only continue if we have at least one mesh in our hierarchy.
            // Otherwise we will resort to box snapping.
            if (Settings.ObjectToObjectSnapMode == ObjectToObjectSnapMode.Vertex && meshObjectsInHierarchy.Count != 0)
            {
                foreach (GameObject gameObject in nearbyObjects)
                {
                    Box objectWorldBox = Box.GetInvalid();

                    // We will first attempt to retrieve the mesh's world box
                    Mesh objectMesh = gameObject.GetMeshFromFilterOrSkinnedMeshRenderer();
                    bool objectHasMesh = objectMesh != null;

                    Octave3DMesh objectOctave3DMesh = Octave3DMeshDatabase.Get().GetOctave3DMesh(objectMesh);
                    if (objectOctave3DMesh == null) objectHasMesh = false;
                    if (objectHasMesh) objectWorldBox = gameObject.GetMeshWorldBox();

                    // If the world box is still invalid, we will acquire the object's sprite world box.
                    if (objectWorldBox.IsInvalid())
                    {
                        if (gameObject.HasSpriteRendererWithSprite()) objectWorldBox = gameObject.GetNonMeshWorldBox();
                    }

                    // If at this point the box is still invalid, it most likely means that the object doesn't have
                    // a mesh or a sprite attached to it, so we will go on to the next iteration of the loop.
                    if (objectWorldBox.IsInvalid()) continue;

                    Box objectQueryBox = objectWorldBox;
                    objectQueryBox.Size += Vector3.one * snapEpsilon;

                    // At this point, we have a valid world box, but we need to check if it comes from a mesh or from 
                    // a sprite. If it's from a mesh, we will perform the vertex-to-vertex snap. Otherwise, we will
                    // snap the vertices of all meshes in our hierarchy to the corner points of the sprite.
                    if(objectHasMesh)
                    {
                        // Detect the object vertices which are overlapped by the hierarchy box
                        List<Vector3> objectOverlappedVerts = objectOctave3DMesh.GetOverlappedWorldVerts(hierarchyQueryBox, gameObject.transform.GetWorldMatrix());

                        // Loop through all meshes in the hierarchy
                        foreach (var meshObjInHierarchy in meshObjectsInHierarchy)
                        {
                            Mesh hierarchyMesh = meshObjInHierarchy.GetMeshFromFilterOrSkinnedMeshRenderer();
                            if (hierarchyMesh == null) continue;

                            Octave3DMesh hierarchyOctave3DMesh = Octave3DMeshDatabase.Get().GetOctave3DMesh(hierarchyMesh);
                            if (hierarchyOctave3DMesh == null) continue;

                            // Detect all vertices inside the mesh which are overlapped by the object's box (inverse operation of what we performed in the
                            // beginning). The idea is to collect all source vertices that could pottentially intersect with each other.
                            List<Vector3> hierarchyMeshOverlappedVerts = hierarchyOctave3DMesh.GetOverlappedWorldVerts(objectQueryBox, meshObjInHierarchy.transform.GetWorldMatrix());
                            foreach (Vector3 hierarchyVertex in hierarchyMeshOverlappedVerts)
                            {
                                foreach (Vector3 objectVertex in objectOverlappedVerts)
                                {
                                    float distance = (hierarchyVertex - objectVertex).magnitude;
                                    if (distance < minDistance)
                                    {
                                        minDistance = distance;
                                        chosenSnapDestination = objectVertex;
                                        chosenSnapPivot = hierarchyVertex;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        List<Vector3> spriteWorldCornerPoints = objectWorldBox.GetCornerPoints();
                        foreach (var meshObjInHierarchy in meshObjectsInHierarchy)
                        {
                            Mesh hierarchyMesh = meshObjInHierarchy.GetMeshFromFilterOrSkinnedMeshRenderer();
                            if (hierarchyMesh == null) continue;

                            Octave3DMesh hierarchyOctave3DMesh = Octave3DMeshDatabase.Get().GetOctave3DMesh(hierarchyMesh);
                            if (hierarchyOctave3DMesh == null) continue;

                            List<Vector3> hierarchyMeshOverlappedVerts = hierarchyOctave3DMesh.GetOverlappedWorldVerts(objectQueryBox, meshObjInHierarchy.transform.GetWorldMatrix());
                            foreach (Vector3 hierarchyVertex in hierarchyMeshOverlappedVerts)
                            {
                                foreach (Vector3 spriteCornerPt in spriteWorldCornerPoints)
                                {
                                    float distance = (hierarchyVertex - spriteCornerPt).magnitude;
                                    if (distance < minDistance)
                                    {
                                        minDistance = distance;
                                        chosenSnapDestination = spriteCornerPt;
                                        chosenSnapPivot = hierarchyVertex;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                List<Vector3> hierarchyBoxCornerPoints = hierarchyWorldBox.GetCornerPoints();
                foreach (GameObject gameObject in nearbyObjects)
                {
                    Box objectWorldBox = Box.GetInvalid();

                    Mesh objectMesh = gameObject.GetMeshFromFilterOrSkinnedMeshRenderer();
                    if (objectMesh != null) objectWorldBox = gameObject.GetMeshWorldBox();
                    if (objectWorldBox.IsInvalid() && gameObject.HasSpriteRendererWithSprite()) objectWorldBox = gameObject.GetNonMeshWorldBox();

                    if (objectWorldBox.IsInvalid()) continue;

                    List<Vector3> worldBoxCornerPoints = objectWorldBox.GetCornerPoints();
                    foreach (Vector3 hierarchyBoxPt in hierarchyBoxCornerPoints)
                    {
                        foreach (Vector3 objectMeshBoxPt in worldBoxCornerPoints)
                        {
                            float distance = (hierarchyBoxPt - objectMeshBoxPt).magnitude;
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                chosenSnapDestination = objectMeshBoxPt;
                                chosenSnapPivot = hierarchyBoxPt;
                            }
                        }
                    }
                }
            }

            if (minDistance < snapEpsilon)
            {
                SnapObjectHierarchyPosition(hierarchyRoot, chosenSnapPivot, chosenSnapDestination, projectedBoxFacePivotPoints, 0.0f);
            }
        }

        public void SnapObjectHierarchyToCenterOfSnapSurface(GameObject hierarchyRoot, Vector3 snapPivotPoint, ProjectedBoxFacePivotPoints projectedBoxFacePivotPoints, float offsetFromSnapSurface)
        {
            _objectSnapSurface.FromMouseCursorRayHit(AcquireCursorRayHit());
            if (_objectSnapSurface.IsValid)
            {
                OrientedBox hierarchyWorldOrientedBox = hierarchyRoot.GetHierarchyWorldOrientedBox();
                if (!hierarchyWorldOrientedBox.IsValid()) return;

                SnapObjectHierarchyPosition(hierarchyRoot, snapPivotPoint, _objectSnapSurface.Center, projectedBoxFacePivotPoints, offsetFromSnapSurface);
            }
        }

        public void SnapObjectPosition(GameObject gameObject)
        {
            _objectSnapSurface.FromMouseCursorRayHit(AcquireCursorRayHit());
            if(_objectSnapSurface.IsValid)
            {
                Transform objectTransform = gameObject.transform;
                objectTransform.position = _objectSnapSurface.GetSnapDestinationPointClosestToCursorPickPoint();
            }
        }

        public void SnapObjectPositionToSnapSurfaceCenter(GameObject gameObject)
        {
            _objectSnapSurface.FromMouseCursorRayHit(AcquireCursorRayHit());
            if (_objectSnapSurface.IsValid)
            {
                Transform objectTransform = gameObject.transform;
                objectTransform.position = _objectSnapSurface.Center;
            }
        }
        #endregion

        #region Private Methods
        private void OnEnable()
        {
            if(!_wasInitialized)
            {
                CoordinateSystemRenderSettings coordSystemRenderSettings = XZSnapGrid.RenderableCoordinateSystem.RenderSettings;
                coordSystemRenderSettings.SetAxisRenderInfinite(CoordinateSystemAxis.PositiveRight, true);
                coordSystemRenderSettings.SetAxisRenderInfinite(CoordinateSystemAxis.NegativeRight, true);
                coordSystemRenderSettings.SetAxisRenderInfinite(CoordinateSystemAxis.PositiveLook, true);
                coordSystemRenderSettings.SetAxisRenderInfinite(CoordinateSystemAxis.NegativeLook, true);

                _wasInitialized = true;
            }
        }

        private MouseCursorRayHit AcquireCursorRayHit()
        {
            MouseCursor.Instance.PushObjectPickMaskFlags(MouseCursorObjectPickFlags.ObjectTerrain | MouseCursorObjectPickFlags.ObjectMesh);
            MouseCursor.Instance.PushObjectMask(ObjectSnapMask);

            MouseCursorRayHit cursorRayHit = MouseCursor.Instance.GetRayHit();

            MouseCursor.Instance.PopObjectPickMaskFlags();
            MouseCursor.Instance.PopObjectMask();

            return cursorRayHit;
        }

        private void SnapObjectHierarchyPosition(GameObject hierarchyRoot, Vector3 snapPivotPoint, Vector3 snapDestinationPoint, ProjectedBoxFacePivotPoints projectedBoxFacePivotPoints, float offsetFromSnapSurface)
        {
            snapDestinationPoint += _objectSnapSurface.Plane.normal * offsetFromSnapSurface;

            Transform hierarchyRootTransform = hierarchyRoot.transform;
            Vector3 snapVector = hierarchyRootTransform.position - snapPivotPoint;
            hierarchyRootTransform.position = snapDestinationPoint + snapVector;
            projectedBoxFacePivotPoints.MovePoints(snapDestinationPoint - snapPivotPoint);
        }

        private void KeepSnappedHierarchyInSnapSurfaceArea(GameObject hierarchyRoot, ProjectedBoxFacePivotPoints projectedHierarchyBoxFacePivotPoints)
        {
            OrientedBox hierarchyWorldOrientedBox = hierarchyRoot.GetHierarchyWorldOrientedBox();
            List<Vector3> worldBoxPoints = hierarchyWorldOrientedBox.GetCenterAndCornerPoints();
            XZOrientedQuad3D snapSurfaceQuad = _objectSnapSurface.SurfaceQuad;

            List<Plane> quadSegmentPlanes = snapSurfaceQuad.GetBoundarySegmentPlanesFacingOutward();
            List<Vector3> pushVectors = new List<Vector3>(quadSegmentPlanes.Count);

            // All box points which are in front of the surface quad's plane are outside
            // the surface so we will have to push them back.
            for(int segmentPlaneIndex = 0; segmentPlaneIndex < quadSegmentPlanes.Count; ++segmentPlaneIndex)
            {
                Plane segmentPlane = quadSegmentPlanes[segmentPlaneIndex];
                Vector3 furthestPointInFront;
                if(segmentPlane.GetFurthestPointInFront(worldBoxPoints, out furthestPointInFront))
                {
                    Vector3 projectedPoint = segmentPlane.ProjectPoint(furthestPointInFront);
                    pushVectors.Add(projectedPoint - furthestPointInFront);
                }
            }

            Transform hierarchyRootTransform = hierarchyRoot.transform;
            foreach(Vector3 pushVector in pushVectors)
            {
                hierarchyRootTransform.position += pushVector;
                projectedHierarchyBoxFacePivotPoints.MovePoints(pushVector);
            }
        }        
        #endregion
    }
}
#endif