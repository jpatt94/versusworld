#if UNITY_EDITOR
using UnityEngine;

namespace O3DWB
{
    public class ObjectVertexSnapSessionRenderer
    {
        #region Public Methods
        public void RenderGizmos(ObjectVertexSnapSession session, ObjectVertexSnapSessionRenderSettings renderSettings)
        {
            if (!session.IsActive) return;

            if(session.SourceGameObject != null)
            {
                if (renderSettings.RenderSourceVertex)
                {
                    Vector2 vertexScreenPos = SceneViewCamera.Camera.WorldToScreenPoint(session.SourceVertex);

                    Circle2D circle = new Circle2D(vertexScreenPos, renderSettings.SourceVertexRadiusInPixels);
                    GizmosEx.Render2DFilledCircle(circle, renderSettings.SourceVertexFillColor);
                    GizmosEx.Render2DCircleBorderLines(circle, renderSettings.SourceVertexBorderColor);
                }
            }
        }
        #endregion
    }
}
#endif