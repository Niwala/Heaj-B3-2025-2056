using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
public class VoxelExplosion : MonoBehaviour
{
    public PlyObject plyObject;
    public VisualEffect visualEffect;

    private GraphicsBuffer voxelPositions;
    private GraphicsBuffer voxelColors;

    private void OnEnable()
    {
        if (plyObject != null && plyObject.IsValid())
        {
            voxelPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, plyObject.positions.Length, sizeof(float) * 3);
            voxelColors = new GraphicsBuffer(GraphicsBuffer.Target.Structured, plyObject.colors.Length, sizeof(float) * 3);

            voxelPositions.SetData(plyObject.positions);
            voxelColors.SetData(plyObject.colors);
        }
    }

    private void OnDisable()
    {
        if (voxelPositions != null)
        {
            voxelPositions.Dispose();
            voxelColors.Dispose();
        }
    }

    private void Update()
    {
        if (voxelPositions != null && visualEffect != null)
        {
            visualEffect.SetGraphicsBuffer("VoxelPositions", voxelPositions);
            visualEffect.SetGraphicsBuffer("VoxelColors", voxelColors);
        }
    }
}
