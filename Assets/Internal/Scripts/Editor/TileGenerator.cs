using UnityEditor;
using UnityEngine;
using System.IO;

namespace Internal.Scripts.Editor
{
    public class TileGenerator
    {
        [MenuItem("Tools/ElecGame/Generate Sample Tiles")]
        public static void GenerateTiles()
        {
            string folderPath = "Assets/Internal/Prefabs/Tiles";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Internal/Prefabs"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Internal"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Internal");
                    }
                    AssetDatabase.CreateFolder("Assets/Internal", "Prefabs");
                }
                AssetDatabase.CreateFolder("Assets/Internal/Prefabs", "Tiles");
            }

            CreateTilePrefab(folderPath, "GroundTile", new Color(0.35f, 0.65f, 0.35f));
            CreateTilePrefab(folderPath, "PathTile", new Color(0.5f, 0.5f, 0.45f));
            CreateTilePrefab(folderPath, "WaterTile", new Color(0.2f, 0.5f, 0.85f));
            CreateWirePrefab(folderPath, "WirePrefab");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Sample Tiles & Wire Generated in " + folderPath);
        }

        private static void CreateWirePrefab(string folder, string name)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<LineRenderer>();
            var wire = go.AddComponent<Internal.Scripts.Core.ElecSystem.Wire>();
            
            // 머터리얼 2종 생성 (URP 호환을 위해 기본 Shader 사용)
            Shader defaultShader = Shader.Find("Universal Render Pipeline/Lit");
            if (defaultShader == null) defaultShader = Shader.Find("Standard");

            Material unpoweredMat = new Material(defaultShader);
            unpoweredMat.color = new Color(0.3f, 0.3f, 0.3f); // 어두운 회색
            unpoweredMat.SetColor("_EmphasisColor", Color.black); // 강조 효과용 (쉐이더에 따라 다름)
            AssetDatabase.CreateAsset(unpoweredMat, Path.Combine(folder, "Wire_Unpowered.mat"));

            Material poweredMat = new Material(defaultShader);
            poweredMat.color = Color.yellow;
            poweredMat.EnableKeyword("_EMISSION"); // 빛나는 효과
            poweredMat.SetColor("_EmissionColor", Color.yellow * 0.5f);
            AssetDatabase.CreateAsset(poweredMat, Path.Combine(folder, "Wire_Powered.mat"));

            // LineRenderer 기본 설정
            var lr = go.GetComponent<LineRenderer>();
            lr.startWidth = 0.2f;
            lr.endWidth = 0.2f;
            lr.sharedMaterial = unpoweredMat;
            lr.textureMode = LineTextureMode.Tile;

            string prefabPath = Path.Combine(folder, name + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

            Object.DestroyImmediate(go);
        }

        private static void CreateTilePrefab(string folder, string name, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            // GridManager.CellSize is 2.0
            go.transform.localScale = new Vector3(2f, 0.1f, 2f);

            Renderer renderer = go.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            
            // Set metalic/smoothness for better look
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Glossiness", 0.2f);

            string matPath = Path.Combine(folder, name + "_Mat.mat");
            AssetDatabase.CreateAsset(mat, matPath);
            renderer.sharedMaterial = mat;

            string prefabPath = Path.Combine(folder, name + ".prefab");
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

            Object.DestroyImmediate(go);
        }
    }
}
