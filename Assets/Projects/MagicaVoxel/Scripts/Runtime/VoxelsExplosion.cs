using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[ExecuteAlways]
public class VoxelsExplosion : MonoBehaviour
{
    public PlyObject plyObject;
    public VisualEffect visualEffect;

    private GraphicsBuffer positionBuffer;
    private GraphicsBuffer colorBuffer;

    private void OnEnable()
    {
        //Create buffers
        if (plyObject != null && plyObject.positions != null && plyObject.positions.Length > 0)
        {
            int count = plyObject.positions.Length;
            positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, sizeof(float) * 3);
            colorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, sizeof(float) * 3);

            positionBuffer.SetData(plyObject.positions);
            colorBuffer.SetData(plyObject.colors);

            if (visualEffect != null)
            {
                visualEffect.SetGraphicsBuffer("VoxelPositions", positionBuffer);
                visualEffect.SetGraphicsBuffer("VoxelColors", colorBuffer);
            }
        }
    }

    private void OnDisable()
    {
        //Release buffers
        positionBuffer?.Release();
        colorBuffer?.Release();
    }

}
