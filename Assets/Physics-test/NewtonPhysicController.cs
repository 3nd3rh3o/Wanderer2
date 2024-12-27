using System.Collections.Generic;
using UnityEngine;

public class NewtonPhysicController : MonoBehaviour
{
    public List<Rigidbody> physicObjects;
    //TODO register planets
    void Start()
    {
        physicObjects.ForEach(o => {o.AddForce(new(), ForceMode.VelocityChange);
        o.AddTorque(new(), ForceMode.VelocityChange);});
    }

    void OnEnable()
    {
        physicObjects = new();
        for (int i = 0; i < gameObject.transform.childCount; i++) physicObjects.Add(gameObject.transform.GetChild(i).GetComponent<Rigidbody>());
    }

    void OnDisable()
    {
        physicObjects = null;
    }

    //TODO apply gravity
    void FixedUpdate()
    {
        for (int i = 0; i < physicObjects.Count; i++)
        {
            Vector3 f = new();
            float m1 = physicObjects[i].mass;
            Vector3 p1 = physicObjects[i].position;
            for (int j = 0; j < physicObjects.Count; j++)
            {
                if (i != j)
                {
                    float m2 = physicObjects[j].mass;
                    Vector3 p2 = physicObjects[j].position;
                    f += - (m1 * m2 / Mathf.Pow((p1 - p2).magnitude, 3)) * (p1 - p2);
                }
            }
            f /= physicObjects.Count - 1;
            physicObjects[i].AddForce(f, ForceMode.Force);
        }
    }


    void Update()
    {

    }
}
