using System.Collections.Generic;
using UnityEngine;
using Internal.Scripts.Core.ElecSystem;

namespace Internal.Scripts.Core.ElecSystem
{
    public class WireDemo : MonoBehaviour
    {
        [SerializeField] private Wire wirePrefab;
        [SerializeField] private Material wireMaterial;
        
        private void Start()
        {
            if (wirePrefab == null)
            {
                GameObject go = new GameObject("WireInstance");
                Wire wire = go.AddComponent<Wire>();
                LineRenderer lr = go.GetComponent<LineRenderer>();
                
                if (wireMaterial != null)
                    lr.material = wireMaterial;
                else
                    lr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                
                lr.startColor = Color.yellow;
                lr.endColor = Color.yellow;
                
                // Demo points
                List<Vector3> demoPoints = new List<Vector3>
                {
                    new Vector3(0, 0.1f, 0),
                    new Vector3(2, 0.1f, 0),
                    new Vector3(2, 0.1f, 2),
                    new Vector3(5, 0.1f, 2),
                    new Vector3(5, 0.1f, 5)
                };
                
                wire.SetPoints(demoPoints);
            }
        }
    }
}
