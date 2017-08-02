#if UNITY_EDITOR
using UnityEngine;

namespace O3DWB
{
    public class GizmoTransformedObjectsMessage : Message
    {
        #region Private Variables
        private ObjectTransformGizmo _objectTransformGizmo;
        #endregion

        #region Public Properties
        public ObjectTransformGizmo ObjectTransformGizmo { get { return _objectTransformGizmo; } }
        #endregion

        #region Constructors
        public GizmoTransformedObjectsMessage(ObjectTransformGizmo objectTransformGizmo)
            : base(MessageType.GizmoTransformedObjects)
        {
            _objectTransformGizmo = objectTransformGizmo;
        }
        #endregion

        #region Public Static Functions
        public static void SendToInterestedListeners(ObjectTransformGizmo objectTransformGizmo)
        {
            var message = new GizmoTransformedObjectsMessage(objectTransformGizmo);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }
}
#endif