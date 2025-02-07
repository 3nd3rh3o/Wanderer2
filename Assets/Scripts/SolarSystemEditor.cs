using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public class SolarSystemEditor : MonoBehaviour
    {
        public Vector3 starPosition;
        public float starMass;
        public float starRadius;

        public int PreviewSamples = 1000;

        public List<PlanetSettings> planets = new();
        private List<GameObject> go;

        void OnEnable()
        {
            go = new(planets.Count);
            planets.ForEach(
                p =>
                {
                    GameObject g = new("planet");
                    g.AddComponent<MeshRenderer>();
                    g.AddComponent<MeshFilter>();
                    g.SetActive(false);
                    g.transform.parent = transform;
                    g.transform.position = p.position;
                    PlanetEditor e = g.AddComponent<PlanetEditor>();
                    e.settings = p;
                    e.lv = p.linearVelocity;
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

        void FixedUpdate()
        {
            PhysicUpdate();
        }

        public void PhysicUpdate()
        {
            go.ForEach(g =>
            {
                PlanetEditor p = g.GetComponent<PlanetEditor>();
                Vector3 O = 100f * starMass / Mathf.Pow((starPosition - p.transform.position).magnitude, 2) * (starPosition - p.transform.position).normalized;

                go.ForEach(g2 =>
                {
                    if (g != g2)
                    {
                        PlanetEditor p2 = g2.GetComponent<PlanetEditor>();
                        O += 100f * p2.settings.mass / Mathf.Pow(0.25f * (p2.transform.position - p.transform.position).magnitude, 2) * (p2.transform.position - p.transform.position).normalized;

                    }

                });
                p.lv += O * Time.deltaTime;
                g.transform.position += p.lv;
            });
        }
        void OnDrawGizmos()
        {
            PlanetEditor[] rbs = new PlanetEditor[transform.childCount];
            Vector3[] pPos = new Vector3[rbs.Length];
            Vector3[] previousForces = new Vector3[rbs.Length];
            for (int i = 0; i < rbs.Length; i++)
            {
                rbs[i] = transform.GetChild(i).GetComponent<PlanetEditor>();
                previousForces[i] = rbs[i].lv;
                pPos[i] = rbs[i].transform.position;
            }


            Gizmos.color = Color.cyan;


            for (int j = 0; j < PreviewSamples; j++)
            {
                for (int i = 0; i < rbs.Length; i++)
                {
                    Vector3 initialPos = pPos[i];
                    PlanetEditor p = rbs[i];
                    Vector3 O = 100f * starMass / Mathf.Pow((starPosition - initialPos).magnitude, 2) * (starPosition - initialPos).normalized;
                    for (int k = 0; k < rbs.Length; k++)
                    {
                        if (k != i)
                            O += 100f * rbs[k].settings.mass / Mathf.Pow(0.25f * (pPos[k] - pPos[i]).magnitude, 2) * (pPos[k] - pPos[i]).normalized;
                    }
                    previousForces[i] += O * Time.fixedDeltaTime;
                    pPos[i] += previousForces[i];
                    Gizmos.DrawLine(initialPos, pPos[i]);
                }
            }
        }
    }
}
