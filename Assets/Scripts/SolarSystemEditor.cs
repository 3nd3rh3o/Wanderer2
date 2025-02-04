using System.Collections.Generic;
using UnityEngine;
using Wanderer;

namespace Wanderer
{
    public class SolarSystemEditor : MonoBehaviour
    {
        public Vector3 starPosition;
        public float starMass;
        public float starRadius;
        public List<PlanetSettings> planets = new();
        private List<GameObject> go;

        void OnEnable()
        {
            go = new(planets.Count);
            planets.ForEach(
                p => {
                    GameObject g = new("planet");
                    g.AddComponent<MeshRenderer>();
                    g.AddComponent<MeshFilter>();
                    g.SetActive(false);
                    g.transform.parent = transform;
                    g.transform.position = p.position;
                    PlanetEditor e = g.AddComponent<PlanetEditor>();
                    e.settings = p;
                    e.DynamicParams = false;
                    go.Add(g);
                    g.SetActive(true);
            });
        }

        void OnDisable()
        {
#if UNITY_ENGINE
            go?.ForEach(g => MonoBehaviour.DestroyImmediate(g));
#else
            go?.ForEach(g => MonoBehaviour.Destroy(g));
#endif
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
