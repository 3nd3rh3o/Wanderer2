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
                p =>
                {
                    GameObject g = new("planet");
                    g.AddComponent<MeshRenderer>();
                    g.AddComponent<MeshFilter>();

                    g.SetActive(false);
                    g.transform.parent = transform;
                    g.transform.position = p.position;
                    Rigidbody r = g.AddComponent<Rigidbody>();
                    r.useGravity = false;
                    r.angularDamping = 0f;
                    r.linearDamping = 0f;
                    r.mass = p.mass;
                    Vector3 O = -g.transform.position.normalized * (p.mass * starMass) / Mathf.Pow((starPosition - g.transform.position).magnitude, 2);
                    r.AddForce(O, ForceMode.Impulse);
                    PlanetEditor e = g.AddComponent<PlanetEditor>();
                    e.settings = p;
                    e.DynamicParams = false;
                    go.Add(g);
                    g.SetActive(true);
                    r.AddTorque(p.angularVelocity * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    r.AddForce(p.linearVelocity * Time.fixedDeltaTime, ForceMode.VelocityChange);
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
            go.ForEach(g =>
            {
                PlanetEditor p = g.GetComponent<PlanetEditor>();
                Rigidbody pr = g.GetComponent<Rigidbody>();
                Vector3 O = -p.transform.position.normalized * (p.settings.mass * starMass) / Mathf.Pow((starPosition - p.transform.position).magnitude, 2);
                pr.AddForce(O * Time.fixedDeltaTime, ForceMode.Impulse);
                go.ForEach(g2 =>
                {
                    if (g != g2)
                    {
                        PlanetEditor p2 = g2.GetComponent<PlanetEditor>();
                        Rigidbody p2r = g.GetComponent<Rigidbody>();
                        Vector3 F = (p.transform.position - p2.transform.position).normalized * (p.settings.mass * p2.settings.mass) / Mathf.Pow((p.transform.position - p.transform.position).magnitude, 2);
                        p2r.AddForce(F * Time.fixedDeltaTime, ForceMode.Impulse);
                    }

                });
            });
        }
    }

}
