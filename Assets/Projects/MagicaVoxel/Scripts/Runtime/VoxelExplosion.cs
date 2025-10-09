using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
public class VoxelExplosion : MonoBehaviour
{
    public PlyObject plyObject;
    public VisualEffect visualEffect;

    private GraphicsBuffer voxelPositions;
    private GraphicsBuffer voxelColors;

    private Hash128 playingHash;

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
        if (plyObject != null && plyObject.IsValid())
        {
            //Rebuild buffer if bad count
            if (voxelPositions != null && voxelPositions.IsValid() && voxelPositions.count != plyObject.count)
            {
                voxelPositions.Release();
                voxelColors.Release();
                voxelPositions = null;
                voxelColors = null;
            }

            //Create buffers if missing
            if (voxelPositions == null)
            {
                voxelPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, plyObject.count, sizeof(float) * 3);
                voxelColors = new GraphicsBuffer(GraphicsBuffer.Target.Structured, plyObject.count, sizeof(float) * 3);
                playingHash = default;
            }

            //Update data
            if (playingHash != plyObject.hash)
            {
                UpdateData();
            }
        }


        if (voxelPositions != null && visualEffect != null && voxelPositions.IsValid())
        {
            visualEffect.SetGraphicsBuffer("VoxelPositions", voxelPositions);
            visualEffect.SetGraphicsBuffer("VoxelColors", voxelColors);
        }
    }

    private void UpdateData()
    {
        voxelPositions.SetData(plyObject.positions);
        voxelColors.SetData(plyObject.colors);
        playingHash = plyObject.hash;
    }
}
