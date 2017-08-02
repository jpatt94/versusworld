#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace O3DWB
{
    [Serializable]
    public class Octave3DScene : IMessageListener
    {
        #region Private Variables
        [SerializeField]
        private GameObjectSphereTree _gameObjectSphereTree = new GameObjectSphereTree(2);
        
        [SerializeField]
        private Octave3DMeshDatabase _octave3DMeshDatabase = new Octave3DMeshDatabase();
        #endregion

        #region Public Static Properties
        public static Vector3 VolumeSizeForNonMeshObjects { get { return Vector3.one; } }
        #endregion

        #region Public Properties
        public Octave3DMeshDatabase Octave3DMeshDatabase { get { return _octave3DMeshDatabase; } }
        #endregion

        #region Constructors
        public Octave3DScene()
        {
            MessageListenerRegistration.PerformRegistrationForOctave3DScene(this);

            // Note: Needed when erasing objects and then Undo. If this step is not performed,
            //       then when the erase operation is undone, the restored objects will not
            //       be registered with the tree.
            Undo.undoRedoPerformed -= RegsiterUnregisteredObjects;
            Undo.undoRedoPerformed += RegsiterUnregisteredObjects;
        }
        #endregion

        #region Public Static Functions
        public static Octave3DScene Get()
        {
            return Octave3DWorldBuilder.ActiveInstance.Octave3DScene;
        }
        #endregion

        #region Public Methods
        public void RenderGizmosDebug()
        {
            _gameObjectSphereTree.RenderGizmosDebug();
        }

        public void Refresh(bool showProgress)
        {
            _gameObjectSphereTree.Rebuild(showProgress);
        }

        public void Update()
        {
            _gameObjectSphereTree.Update();
        }

        public void OnSceneGUI()
        {
            _gameObjectSphereTree.OnSceneGUI();
        }

        public List<GameObjectRayHit> RaycastAllBox(Ray ray)
        {
            return _gameObjectSphereTree.RaycastAllBox(ray);
        }

        public List<GameObjectRayHit> RaycastAllSprite(Ray ray)
        {
            return _gameObjectSphereTree.RaycastAllSprite(ray);
        }

        public List<GameObjectRayHit> RaycastAllMesh(Ray ray)
        {
            return _gameObjectSphereTree.RaycastAllMesh(ray);
        }

        public List<GameObject> OverlapSphere(Sphere sphere)
        {
            return _gameObjectSphereTree.OverlapSphere(sphere);
        }

        public List<GameObject> OverlapBox(OrientedBox box)
        {
            return _gameObjectSphereTree.OverlapBox(box);
        }

        public List<GameObject> OverlapBox(Box box)
        {
            return _gameObjectSphereTree.OverlapBox(box);
        }

        public bool BoxIntersectsAnyObjectBoxes(OrientedBox box, List<GameObject> ignoreObjects, bool allowFaceTouch)
        {
            if (ignoreObjects == null) ignoreObjects = new List<GameObject>();
            return _gameObjectSphereTree.BoxIntersectsAnyObjectBoxes(box, new HashSet<GameObject>(ignoreObjects), allowFaceTouch);
        }

        public bool ObjectMeshIntersectsAnyMesh(GameObject queryMeshObject, TransformMatrix worldMatrix, List<GameObject> ignoreObjects)
        {
            if (ignoreObjects == null) ignoreObjects = new List<GameObject>();

            Octave3DMesh octave3DMesh = queryMeshObject.GetOctave3DMesh();
            if (octave3DMesh == null) return false;

            return _gameObjectSphereTree.ObjectMeshIntersectsAnyMesh(octave3DMesh, worldMatrix, new HashSet<GameObject>(ignoreObjects));
        }

        public bool ObjectMeshInHierarchyIntersectsAnyMesh(GameObject queryRoot, TransformMatrix rootWorldMatrix, List<GameObject> ignoreObjects)
        {
            List<GameObject> allChildrenAndSelf = queryRoot.GetAllChildrenIncludingSelf();

            foreach(var child in allChildrenAndSelf)
            {
                if(child == queryRoot)
                {
                    if (ObjectMeshIntersectsAnyMesh(child, rootWorldMatrix, ignoreObjects)) return true;
                }
                else
                {
                    Matrix4x4 relativeTransform = child.transform.GetRelativeTransformMatrix(rootWorldMatrix).ToMatrix4x4x;
                    Matrix4x4 childWorldTransform = rootWorldMatrix.ToMatrix4x4x * relativeTransform;
                    if (ObjectMeshIntersectsAnyMesh(child, new TransformMatrix(childWorldTransform), ignoreObjects)) return true;
                }
            }

            return false;
        }

        public List<GameObject> InstantiateObjectHirarchiesFromPlacementDataCollection(List<ObjectPlacementData> objectPlacementDataCollection)
        {
            if (objectPlacementDataCollection.Count == 0) return new List<GameObject>();

            var instantiatedHierarchyRoots = ObjectInstantiation.InstantiateObjectHirarchiesFromPlacementDataCollection(objectPlacementDataCollection);
            _gameObjectSphereTree.RegisterGameObjectHierarchies(instantiatedHierarchyRoots);
            return instantiatedHierarchyRoots;
        }

        public GameObject InstantiateObjectHierarchyFromPlacementData(ObjectPlacementData objectPlacementData)
        {
            GameObject instantiatedObject = ObjectInstantiation.InstantiateObjectHierarchyFromPlacementData(objectPlacementData);
            _gameObjectSphereTree.RegisterGameObjectHierarchy(instantiatedObject);
            return instantiatedObject;
        }

        public GameObject InstantiateObjectHierarchyFromPrefab(Prefab prefab, Transform transformData)
        {
            GameObject instantiatedObject = ObjectInstantiation.InstantiateObjectHierarchyFromPrefab(prefab, transformData.position, transformData.rotation, transformData.lossyScale);
            _gameObjectSphereTree.RegisterGameObjectHierarchy(instantiatedObject);
            return instantiatedObject;
        }

        public GameObject InstantiateObjectHierarchyFromPrefab(Prefab prefab, Vector3 worldPosition, Quaternion worldRotation, Vector3 worldScale)
        {
             GameObject instantiatedObject = ObjectInstantiation.InstantiateObjectHierarchyFromPrefab(prefab, worldPosition, worldRotation, worldScale);
             _gameObjectSphereTree.RegisterGameObjectHierarchy(instantiatedObject);
             return instantiatedObject;
        }

        public void RegisterObjectHierarchy(GameObject hierarchyRoot)
        {
            _gameObjectSphereTree.RegisterGameObjectHierarchy(hierarchyRoot);
        }
       
        public void RegisterObjectHierarchies(List<GameObject> roots)
        {
            foreach(var root in roots)
            {
                RegisterObjectHierarchy(root);
            }
        }

        public void RegisterObjectCollection(List<GameObject> gameObjects)
        {
            _gameObjectSphereTree.RegisterGameObjectCollection(gameObjects);
        }

        public void RegsiterUnregisteredObjects()
        {
            _gameObjectSphereTree.RegisterUnregisteredObjects();
        }
        #endregion

        #region Message Handlers
        public void RespondToMessage(Message message)
        {
            switch(message.Type)
            {
                case MessageType.ToolWasReset:

                    RespondToMessage(message as ToolWasResetMessage);
                    break;

                case MessageType.ToolWasSelected:

                    RespondToMessage(message as ToolWasSelectedMessage);
                    break;
            }
        }

        private void RespondToMessage(ToolWasResetMessage message)
        {
            var allWorkingObjects = Octave3DWorldBuilder.ActiveInstance.GetAllWorkingObjects();
            RegisterObjectCollection(allWorkingObjects);
        }

        private void RespondToMessage(ToolWasSelectedMessage message)
        {
            // Note: It is possible that the user may have changed the transform of the
            //       objects while the tool was deselected, so we need to ensure that the
            //       tree is up to date.
            _gameObjectSphereTree.HandleTransformChangesForAllRegisteredObjects();
      
            // It may be possible that the user has added objects to the scene while the tool object
            // was deselected, so we will instruct the camera to update its visibility.
            SceneViewCamera.Instance.SetObjectVisibilityDirty();
        }
        #endregion
    }
}
#endif