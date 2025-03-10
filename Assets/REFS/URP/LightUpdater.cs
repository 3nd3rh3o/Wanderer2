using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class LightUpdater : MonoBehaviour
{
    public Transform SunTransform;
    public Material mat;
    private Material mI;
    // Update is called once per frame
    void Update()
    {
        if (!SunTransform || !mat) return;
        if (mI==null) mI = Instantiate(mat);
        GetComponent<MeshRenderer>().sharedMaterial = mI;
        mI.SetVector("_Position", transform.position);
        mI.SetVector("_LightPosition", SunTransform.position);
        GameObject[] go = GameObject.FindGameObjectsWithTag("PlanetCaster");
        Vector4[] pos = new Vector4[20];
        float[] radius = new float[20];
        for (int i = 0; i < 20; i++)
        {
            Transform p = i >= go.Length ? null : go[i].transform;
            pos[i] = i >= go.Length ? new() : p.position;
            radius[i] = i >= go.Length ? -1f : p.lossyScale.x;
        }
        mI.SetFloatArray("_OtherPlanetsRadius", radius);
        mI.SetVectorArray("_OtherPlanetsPos", pos);
    }
}
