using ECM.Characters;
using ECM.Characters.Controllers;
using UnityEditor;
using UnityEngine;

namespace ECM.Editor
{
    public static class ECMMenu
    {
        [MenuItem("GameObject/ECM/New Capsule Character", false, 0)]
        public static void CreateBaseCharacter()
        {
            // Instance Game object with required character's components

            var gameObject = new GameObject("ECM_CapsuleCharacter", typeof (Rigidbody), typeof (CapsuleCollider),
                typeof (SphereGroundDetection), typeof (CharacterMovement), typeof (BaseCharacterController));

            // Initialize rigidbody

            var rb = gameObject.GetComponent<Rigidbody>();

            rb.useGravity = false;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.freezeRotation = true;

            // Initialize its collider, attempts to load supplied frictionless material

            var capsuleCollider = gameObject.GetComponent<CapsuleCollider>();

            capsuleCollider.height = 2.0f;
            capsuleCollider.material = (PhysicMaterial) Resources.Load("PhysicMaterials/Frictionless");

            var physicMaterial = capsuleCollider.sharedMaterial;
            if (physicMaterial != null)
                return;

            // if not founded, instantiate one and logs a warning message

            physicMaterial = new PhysicMaterial("Frictionless")
            {
                dynamicFriction = 0.0f,
                staticFriction = 0.0f,
                bounciness = 0.0f,
                frictionCombine = PhysicMaterialCombine.Multiply,
                bounceCombine = PhysicMaterialCombine.Multiply
            };

            capsuleCollider.material = physicMaterial;

            Debug.LogWarning(
                string.Format(
                    "CharacterMovement: No 'PhysicMaterial' found for {0} CapsuleCollider, a frictionless one has been created and assigned.\n You should add a Frictionless 'PhysicMaterial' to game object {0}.",
                    gameObject.name));
        }

        [MenuItem("GameObject/ECM/New Box Character", false, 0)]
        public static void CreateBoxBaseCharacter()
        {
            // Instance Game object with required character's components

            var gameObject = new GameObject("ECM_BoxCharacter", typeof(Rigidbody), typeof(BoxCollider),
                typeof(BoxGroundDetection), typeof(CharacterMovement), typeof(BaseCharacterController));

            // Initialize rigidbody

            var rb = gameObject.GetComponent<Rigidbody>();

            rb.useGravity = false;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.freezeRotation = true;

            // Initialize its collider, attempts to load supplied frictionless material

            var boxCollider = gameObject.GetComponent<BoxCollider>();

            boxCollider.size = new Vector3(1.0f, 2.0f, 1.0f);
            boxCollider.material = (PhysicMaterial)Resources.Load("PhysicMaterials/Frictionless");

            var physicMaterial = boxCollider.sharedMaterial;
            if (physicMaterial != null)
                return;

            // if not founded, instantiate one and logs a warning message

            physicMaterial = new PhysicMaterial("Frictionless")
            {
                dynamicFriction = 0.0f,
                staticFriction = 0.0f,
                bounciness = 0.0f,
                frictionCombine = PhysicMaterialCombine.Multiply,
                bounceCombine = PhysicMaterialCombine.Multiply
            };

            boxCollider.material = physicMaterial;

            Debug.LogWarning(
                string.Format(
                    "CharacterMovement: No 'PhysicMaterial' found for {0} CapsuleCollider, a frictionless one has been created and assigned.\n You should add a Frictionless 'PhysicMaterial' to game object {0}.",
                    gameObject.name));
        }
    }
}