using SamsBackpack.SubstanceReimporter;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace Heaj.LevelSelector
{
    public class LevelSelection : MonoBehaviour
    {
        [Min(0)]
        public int worldDepth = 10;

        [Min(2)]
        public int maxWidth = 3;

        public float padding = 0.1f;

        public int resolution = 4096;

        public const float relaxStepForce = 0.3f;
        public const int relaxStepCount = 60;

        private List<LevelInfo> levels;
        private List<LevelLink> links;
        private Rect bounds;

        private Vector3 center => new Vector3(bounds.center.x, 0, bounds.center.y);
        private Vector3 size => new Vector3(bounds.size.x, 0.01f, bounds.size.y);

        public bool generate;
        public bool saveExample;

        public ComputeShader mapGeneratorShader;
        public RenderTexture mapSourceTex;
        public RoomChances roomDatabase;
        public SubstanceGraph substance;

        public Transform sourcePlane;
        public Material sourceMaterial;

        public Transform resultPlane;
        public Material resultMaterial;

        public float iconSize = 0.05f;
        public float lineThickness = 0.01f;

        private void OnValidate()
        {
            if (generate)
            {
                generate = false;
                Generate();
                Relax();
                ComputeBounds();
                Render();
                ExecuteSubstance();

                sourcePlane.transform.position = center + Vector3.right * (worldDepth + 1 + padding);
                sourcePlane.transform.localScale = size * 0.1f;

                resultPlane.transform.position = center + Vector3.right * (worldDepth + 1 + padding) * 2;
                resultPlane.transform.localScale = size * 0.1f;
            }

            if (saveExample)
            {
                saveExample = false;
                SaveExample();
            }
        }

        private void Start()
        {
            Generate();
        }

        private void Generate()
        {
            levels = new List<LevelInfo>();
            links = new List<LevelLink>();

            int center = Mathf.RoundToInt(maxWidth * 0.5f + Random.Range(-0.1f, 0.1f));

            //Create start 
            levels.Add(new LevelInfo(LevelType.Start, 0, center));
            int[] currentGroup = EmtpyGroup();
            currentGroup[center] = 0;


            //Create levels
            for (int depth = 0; depth < worldDepth; depth++)
            {
                currentGroup = NextStep(currentGroup, depth + 1, depth == 0);
            }


            //Create end
            levels.Add(new LevelInfo(LevelType.BigBoss, worldDepth + 1, center));
            for (int i = 0; i < currentGroup.Length; i++)
            {
                if (currentGroup[i] != -1)
                    links.Add(new LevelLink(currentGroup[i], levels.Count - 1));
            }
        }

        private void Relax()
        {
            //Virtual links : Add a few links for a more stable result.
            List<LevelLink> virtualLinks = new List<LevelLink>();
            virtualLinks.AddRange(links);
            int previousDepth = -1;
            int previousDepthID = 0;
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].depth == previousDepth)
                {
                    virtualLinks.Add(new LevelLink(previousDepthID, i));
                }

                previousDepth = levels[i].depth;
                previousDepthID = i;
            }


            Vector2[] offsets = new Vector2[levels.Count];

            for (int j = 0; j < relaxStepCount; j++)
            {
                //Compute delta in offsets
                for (int i = 0; i < virtualLinks.Count; i++)
                {
                    LevelLink l = virtualLinks[i];
                    Vector2 offset = levels[l.from].center - levels[l.to].center;

                    float length = offset.magnitude;
                    offset = (offset / length) * (length - 1.0f);

                    offsets[l.from] -= offset * relaxStepForce;
                    offsets[l.to] += offset * relaxStepForce;
                }

                //Apply offsets
                for (int i = 1; i < offsets.Length - 1; i++)
                {
                    LevelInfo level = levels[i];
                    level.center += offsets[i];
                    levels[i] = level;
                }

                //Reset offsets
                for (int i = 0; i < offsets.Length; i++)
                {
                    offsets[i] = default;
                }
            }
        }

        private void ComputeBounds()
        {
            if (levels.Count == 0)
                return;

            Vector2 min = levels[0].center;
            Vector2 max = levels[0].center;

            for (int i = 1; i < levels.Count; i++)
            {
                Vector2 c = levels[i].center;
                min.x = Mathf.Min(c.x, min.x);
                min.y = Mathf.Min(c.y, min.y);
                max.x = Mathf.Max(c.x, max.x);
                max.y = Mathf.Max(c.y, max.y);
            }

            bounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

            //Square bounds
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y) + padding;
            Vector2 size = new Vector2(maxSize, maxSize);
            bounds = new Rect(bounds.center - size * 0.5f, size);
        }

        private void Render()
        {
            if (mapSourceTex == null)
            {
                mapSourceTex = new RenderTexture(resolution, resolution, 0);
                mapSourceTex.enableRandomWrite = true;
                mapSourceTex.name = "MapSource";
            }


            //Randomize rooms
            int[] unorderedRoomIDs = new int[levels.Count - 2]; //-2 : Don't include start & end
            for (int i = 0; i < unorderedRoomIDs.Length; i++)
                unorderedRoomIDs[i] = i + 1; // + 1 : Avoid start
            unorderedRoomIDs = unorderedRoomIDs.OrderBy(x => Random.Range(0.0f, 1.0f)).ToArray();

            //Assign required rooms and build chance table for default rooms
            List<Vector2Int> chanceTable = new List<Vector2Int>();
            List<int> requiredRooms = new List<int>();
            Dictionary<int, int> roomMap = new Dictionary<int, int>();

            int chancesCount = 0;
            int assignedRoomsCursor = 0;
            for (int i = 0; i < roomDatabase.rooms.Length; i++)
            {
                Room room = roomDatabase.rooms[i];

                switch (room.type)
                {
                    case RoomType.Start: roomMap.Add(0, i); break;
                    case RoomType.End: roomMap.Add(levels.Count - 1, i); break;

                    case RoomType.Default:
                            chancesCount += room.chances;
                            chanceTable.Add(new Vector2Int(chancesCount, i));
                        break;

                    case RoomType.Required:
                        int count = Random.Range(room.requireCount.x, room.requireCount.y);
                        for (int j = 0; j < count; j++)
                        {
                            if (assignedRoomsCursor >= unorderedRoomIDs.Length)
                            {
                                Debug.LogError("Not enough room in level for all the required rooms.");
                                continue;
                            }
                            roomMap.Add(unorderedRoomIDs[assignedRoomsCursor++], i);
                        }
                     break;
                }
            }

            //Assign default rooms
            for (int i = assignedRoomsCursor; i < unorderedRoomIDs.Length; i++)
            {
                //Get random room
                int rnd = Random.Range(0, chancesCount);
                int j = 0;
                while (rnd > chanceTable[j].x)
                    j++;
                roomMap.Add(unorderedRoomIDs[i], chanceTable[j].y);
            }

            //Icon buffer
            ComputeBuffer iconBuffer = new ComputeBuffer(levels.Count, IconInfo.stride);
            IconInfo[] icons = new IconInfo[levels.Count];
            for (int i = 0; i < levels.Count; i++)
            {
                icons[i] = new IconInfo
                {
                    center = Rect.PointToNormalized(bounds, levels[i].center),
                    size = iconSize,
                    tileCoords = roomDatabase.GetRoomUvs(roomMap[i])
                };
            }
            iconBuffer.SetData(icons);


            //Line buffer
            ComputeBuffer capsuleBuffer = new ComputeBuffer(links.Count, CapsuleInfo.stride);
            CapsuleInfo[] capsules = new CapsuleInfo[links.Count];
            for (int i = 0; i < links.Count; i++)
            {
                capsules[i] = new CapsuleInfo
                {
                    a = Rect.PointToNormalized(bounds, levels[links[i].from].center),
                    b = Rect.PointToNormalized(bounds, levels[links[i].to].center),
                };
            }
            capsuleBuffer.SetData(capsules);


            int kernel = 0;
            mapGeneratorShader.SetInt("_IconCount", levels.Count);
            mapGeneratorShader.SetInt("_CapsuleCount", links.Count);
            mapGeneratorShader.SetFloat("_LineThickness", lineThickness);
            mapGeneratorShader.SetVector("_InvTexSize", new Vector2(1.0f / mapSourceTex.width, 1.0f / mapSourceTex.height));
            mapGeneratorShader.SetBuffer(kernel, "_Icons", iconBuffer);
            mapGeneratorShader.SetBuffer(kernel, "_Capsules", capsuleBuffer);
            mapGeneratorShader.SetTexture(kernel, "_IconSource", roomDatabase.texture);
            mapGeneratorShader.SetTexture(kernel, "_Result", mapSourceTex);

            mapGeneratorShader.Dispatch(kernel, Mathf.CeilToInt(mapSourceTex.width / 8.0f), Mathf.CeilToInt(mapSourceTex.height / 8.0f), 1);
            sourceMaterial.SetTexture("_BaseMap", mapSourceTex);

            iconBuffer.Release();
            capsuleBuffer.Release();
        }

        private void ExecuteSubstance()
        {
            Texture2D tempTex = new Texture2D(mapSourceTex.width, mapSourceTex.height);
            RenderTexture.active = mapSourceTex;
            tempTex.ReadPixels(new Rect(0, 0, tempTex.width, tempTex.height), 0, 0);
            tempTex.Apply();
            RenderTexture.active = null;

            Debug.Log(tempTex.width);
            substance.SetOutputSize(tempTex.width, tempTex.height);
            substance.SetTexture("Source", tempTex);
            substance.Render(null);
            resultMaterial.SetTexture("_BaseMap", substance.GetOutput(0));
            //substance.SetInputTexture("Source", tempTex);
            //substance.Render();
            //Texture2D result = substance.GetOutputTexture("Result");

            //resultMaterial.SetTexture("_BaseMap", result);
        }

        private void SaveExample()
        {
            Texture2D tempTex = new Texture2D(mapSourceTex.width, mapSourceTex.height);
            RenderTexture.active = mapSourceTex;
            tempTex.ReadPixels(new Rect(0, 0, tempTex.width, tempTex.height), 0, 0);
            tempTex.Apply();
            RenderTexture.active = null;

            File.WriteAllBytes("Assets/Example.tga", tempTex.EncodeToTGA());
        }

        private int[] EmtpyGroup()
        {
            int[] group = new int[maxWidth];
            for (int i = 0; i < maxWidth; i++)
                group[i] = -1;
            return group;
        }

        private int[] NextStep(int[] current, int depth, bool forceSplit = false)
        {
            int[] next = new int[current.Length];
            int pathCount = 0;

            for (int i = 0; i < current.Length; i++)
            {
                next[i] = -1;
                if (current[i] != -1)
                    pathCount++;
            }

            forceSplit |= (pathCount == 1);
            int previousHeight = -1;

            bool HasPath(int height)
            {
                if (height < 0 || height >= maxWidth)
                    return false;
                return next[height] != -1;
            }

            bool GetOrAddLevel(int height, out int levelID)
            {
                if (height < 0 || height >= maxWidth || height < previousHeight)
                {
                    levelID = -1;
                    return false;
                }

                if (next[height] == -1)
                {
                    next[height] = levels.Count;
                    levels.Add(new LevelInfo(LevelType.Combat, depth, height));
                }
                previousHeight = height;
                levelID = next[height];
                return true;
            }

            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] != -1)
                {
                    bool hasPath = false;

                    //Got top
                    int hasTopConnection = HasPath(i - 1) ? 1 : 0;
                    if (forceSplit || Random.Range(0, 2 + hasTopConnection) <= 0 + hasTopConnection)
                    {
                        if (GetOrAddLevel(i - 1, out int newLevelID))
                        {
                            links.Add(new LevelLink(current[i], newLevelID));
                            hasPath = true;
                        }
                    }

                    //Go bottom
                    int hasBotConnection = HasPath(i - 1) ? 1 : 0;
                    if (forceSplit || Random.Range(0, 2 + hasBotConnection) <= 0 + hasBotConnection)
                    {
                        if (GetOrAddLevel(i + 1, out int newLevelID))
                        {
                            links.Add(new LevelLink(current[i], newLevelID));
                            hasPath = true;
                        }
                    }

                    //Go straight
                    if (forceSplit || !hasPath || Random.Range(0, 2) == 0)
                    {
                        if (GetOrAddLevel(i, out int newLevelID))
                        {
                            links.Add(new LevelLink(current[i], newLevelID));
                        }
                    }
                }
            }

            return next;
        }

        private void OnDrawGizmos()
        {
            if (levels != null)
            {
                Gizmos.color = Color.cyan;

                foreach (var level in levels)
                {
                    Gizmos.DrawSphere(level.position, 0.1f);
                }

                foreach (var link in links)
                {
                    Gizmos.DrawLine(levels[link.from].position, levels[link.to].position);
                }
            }

            //Draw bounds
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);

        }

        struct IconInfo
        {
            public Vector2 center;
            public float size;
            public Vector4 tileCoords;

            public static int stride => sizeof(float) * 7;
        }

        struct CapsuleInfo
        {
            public Vector2 a;
            public Vector2 b;

            public static int stride => sizeof(float) * 4;
        }
    }
}