using Unity.Mathematics;
using UnityEngine;

namespace Wanderer
{
    public static class CBuffHelper
    {
        public static ComputeBuffer Vec3Buff(Vector3[] v)
        {
            int count = v.Length;
            ComputeBuffer buff = new(v.Length, sizeof(float)*3);
            float3[] vB = new float3[v.Length];
            for(int i = 0; i < count; i++) vB[i]=v[i];
            buff.SetData(vB);
            return buff;
        }

        public static ComputeBuffer ColBuff(Color[] c)
        {
            int count = c.Length;
            ComputeBuffer buff = new(c.Length, sizeof(float)*4);
            float4[] vB = new float4[c.Length];
            for(int i = 0; i < count; i++) vB[i]=new float4(c[i].r, c[i].g, c[i].b, c[i].a);
            buff.SetData(vB);
            return buff;
        }

        public static Vector3[] ExtractVec3Buff(ComputeBuffer buff)
        {
            Vector3[] v = new Vector3[buff.count];
            buff.GetData(v);
            return v;
        }
        
    }
}