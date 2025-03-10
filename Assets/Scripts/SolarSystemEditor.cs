using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wanderer
{
    public class SolarSystemEditor : MonoBehaviour
    {
        public Vector3 starPosition;
        private Vector3 reelStarPos;
        public float starMass;
        public float starRadius;
        [Range(0.01f, 4f)] public float simSpeed = 1;

        public int PreviewSamples = 1000;
        [Range(0, 10)] public int focus;
        public Material shadowMat;

        public List<PlanetSettings> planets = new();
        private List<GameObject> go;

        void OnEnable()
        {
            go = new(planets.Count);
            for (int i = 0; i < planets.Count; i++)
            {
                PlanetSettings p = planets[i];
                GameObject g = new("planet");
                StaticPlanetInstantier.SpawnPlanet(g, p, transform, i + 1, shadowMat);
                go.Add(g);
            }
        }

        void OnDisable()
        {
#if UNITY_ENGINE
            
            go?.ForEach(g => {
                while(g.transform.childCount > 0) MonoBehaviour.DestroyImmediate(g.transform.GetChild(0));
            });
            while(transform.childCount > 0) MonoBehaviour.DestroyImmediate(transform.GetChild(0));
            go = new();
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
            if (focus == 0)
            {
                transform.position = -starPosition;
                reelStarPos = starPosition;
            }
            else if (focus <= go.Count)
            {
                transform.position = -go[focus - 1].transform.localPosition;
                reelStarPos = starPosition - transform.localPosition;
            }
            else
            {
                focus = go.Count;
            }
            go.ForEach(g =>
            {
                PlanetEditor p = g.GetComponent<PlanetEditor>();
                Vector3 O = (simSpeed * 100f) * starMass / Mathf.Pow(((starPosition) - p.transform.localPosition).magnitude, 2) * ((starPosition) - p.transform.localPosition).normalized;
                p.transform.localRotation = (Quaternion.Euler(p.settings.rotation) * Quaternion.Euler(simSpeed * Time.fixedDeltaTime * p.settings.angularVelocity) * Quaternion.Inverse(Quaternion.Euler(p.settings.rotation)) * p.transform.localRotation).normalized;

                go.ForEach(g2 =>
                {
                    if (g != g2)
                    {
                        PlanetEditor p2 = g2.GetComponent<PlanetEditor>();
                        O += (simSpeed * 100f) * p2.settings.mass / Mathf.Pow(0.25f * (p2.transform.localPosition - p.transform.localPosition).magnitude, 2) * (p2.transform.localPosition - p.transform.localPosition).normalized;

                    }

                });
                p.lv += O * Time.deltaTime;
                g.transform.localPosition += p.lv * simSpeed;
            });
        }
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            PlanetEditor[] rbs = new PlanetEditor[go.Count];
            Vector3[] pPos = new Vector3[rbs.Length];
            Vector3[] previousForces = new Vector3[rbs.Length];
            for (int i = 0; i < rbs.Length; i++)
            {
                rbs[i] = go[i].GetComponent<PlanetEditor>();
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
                    Vector3 O = (simSpeed * 100f) * starMass / Mathf.Pow(((starPosition - transform.localPosition) - initialPos).magnitude, 2) * ((starPosition - transform.localPosition) - initialPos).normalized;
                    for (int k = 0; k < rbs.Length; k++)
                    {
                        if (k != i)
                            O += (simSpeed * 100f) * rbs[k].settings.mass / Mathf.Pow(0.25f * (pPos[k] - pPos[i]).magnitude, 2) * (pPos[k] - pPos[i]).normalized;
                    }
                    previousForces[i] += O * Time.fixedDeltaTime;
                    pPos[i] += previousForces[i] * simSpeed;
                    if (focus == 0)
                    {
                        Gizmos.DrawLine(initialPos, pPos[i]);
                    }
                    else
                    {
                        Gizmos.DrawLine(initialPos - pPos[focus - 1], pPos[i] - pPos[focus - 1]);
                    }

                }
            }
        }
    }
}
