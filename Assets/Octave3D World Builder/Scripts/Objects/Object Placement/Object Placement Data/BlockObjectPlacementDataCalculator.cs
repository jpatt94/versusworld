#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace O3DWB
{
    public class BlockObjectPlacementDataCalculator
    {
        #region Private Variables
        private ObjectPlacementBlock _block;
        #endregion

        #region Public Properties
        public ObjectPlacementBlock Block { get { return _block; } set { _block = value; } }
        #endregion

        #region Public Methods
        public List<ObjectPlacementData> Calculate()
        {
            if (_block == null || _block.NumberOfSegments == 0 || !ObjectPlacementGuide.ExistsInScene) return new List<ObjectPlacementData>();

            Prefab placementGuidePrefab = ObjectPlacementGuide.Instance.SourcePrefab;
            Vector3 placementGuideWorldScale = ObjectPlacementGuide.Instance.WorldScale;
            Quaternion placementGuideWorldRotation = ObjectPlacementGuide.Instance.WorldRotation;
            float objectMissChance = _block.Settings.ManualConstructionSettings.ObjectMissChance;
            ObjectRotationRandomizationSettings blockObjectRotationRandomizationSettings = _block.Settings.ManualConstructionSettings.ObjectRotationRandomizationSettings;
            bool randomizeRotations = blockObjectRotationRandomizationSettings.RandomizeRotation;
            Vector3 objectOffsetAlongExtensionPlaneNormal = _block.Settings.ManualConstructionSettings.OffsetAlongGrowDirection * _block.ExtensionPlane.normal;
            bool allowObjectIntersection = ObjectPlacementSettings.Get().ObjectIntersectionSettings.AllowIntersectionForBlockPlacement;
            bool randomizePrefabs = _block.Settings.ManualConstructionSettings.RandomizePrefabs;
            PrefabCategory activePrefabCategory = PrefabCategoryDatabase.Get().ActivePrefabCategory;

            var objectPlacementDataInstances = new List<ObjectPlacementData>(_block.NumberOfSegments * 10);
            for (int segmentIndex = 0; segmentIndex < _block.NumberOfSegments; ++segmentIndex)
            {
                ObjectPlacementBoxStackSegment segment = _block.GetSegmentByIndex(segmentIndex);
                for (int stackIndex = 0; stackIndex < segment.NumberOfStacks; ++stackIndex)
                {
                    ObjectPlacementBoxStack stack = segment.GetStackByIndex(stackIndex);
                    if (stack.IsOverlappedByAnotherStack) continue;

                    for (int stackBoxIndex = 0; stackBoxIndex < stack.NumberOfBoxes; ++stackBoxIndex)
                    {
                        ObjectPlacementBox box = stack.GetBoxByIndex(stackBoxIndex);
                        if (box.IsHidden) continue;

                        if (ObjectPlacementMissChance.Missed(objectMissChance, ObjectPlacementBlockManualConstructionSettings.MinObjectMissChance, ObjectPlacementBlockManualConstructionSettings.MaxObjectMissChance)) continue;

                        if (!allowObjectIntersection && ObjectQueries.IntersectsAnyObjectsInScene(box.OrientedBox, true)) continue;

                        Quaternion worldRotation = placementGuideWorldRotation;
                        if (randomizeRotations) worldRotation = ObjectRotationRandomization.GenerateRandomRotationQuaternion(blockObjectRotationRandomizationSettings);

                        Vector3 worldScale = placementGuideWorldScale;
                        Prefab prefab = placementGuidePrefab;
                        if (randomizePrefabs && activePrefabCategory.NumberOfPrefabs != 0)
                        {
                            int randomPrefabIndex = UnityEngine.Random.Range(0, activePrefabCategory.NumberOfPrefabs);
                            Prefab randomPrefab = activePrefabCategory.GetPrefabByIndex(randomPrefabIndex);
                            if(randomPrefab != null && randomPrefab.UnityPrefab != null)
                            {
                                prefab = activePrefabCategory.GetPrefabByIndex(randomPrefabIndex);
                                worldScale = prefab.UnityPrefab.transform.lossyScale;
                            }
                        }

                        var objectPlacementData = new ObjectPlacementData();
                        objectPlacementData.WorldPosition = ObjectPositionCalculator.CalculateObjectHierarchyPosition(prefab, box.Center + objectOffsetAlongExtensionPlaneNormal, worldScale, placementGuideWorldRotation);
                        objectPlacementData.WorldScale = worldScale;
                        objectPlacementData.WorldRotation = worldRotation;
                        objectPlacementData.Prefab = prefab;
                        objectPlacementDataInstances.Add(objectPlacementData);
                    }
                }
            }

            return objectPlacementDataInstances;
        }
        #endregion
    }
}
#endif