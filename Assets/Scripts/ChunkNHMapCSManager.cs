using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;


public static class ChunkNHMapCSManagerUtils
{   const int MAX_INSTR = 4;
    public static ChunkNHMapCSManager.Instr.MODIFICATION[] OnlyInstr(this ChunkNHMapCSManager.Instr[] instr)
    {
        ChunkNHMapCSManager.Instr.MODIFICATION[] res = new ChunkNHMapCSManager.Instr.MODIFICATION[instr.Length];
        for (int i = 0; i < instr.Length; i++)
        {
            res[i] = instr[i].GetInstr();
        }
        return res;
    }
    public static Vector4[] UnPackArgs(this ChunkNHMapCSManager.Instr[] instr)
    {
        Vector4[] res = new Vector4[MAX_INSTR*2];
        for (int i = 0; i < instr.Length; i++) (res[2*i], res[2*i+1])= (instr[i].args[0], instr[i].args[1]);
        return res;
    }
    public static int UnPackInstr(this ChunkNHMapCSManager.Instr[] instrs)
    {
        ChunkNHMapCSManager.Instr.MODIFICATION[] instr = instrs.OnlyInstr();

        if (instr.Length > MAX_INSTR + 1)
        {
            List<ChunkNHMapCSManager.Instr.MODIFICATION> m = instr.ToList();
            m.RemoveRange(MAX_INSTR, instr.Length - MAX_INSTR);
            instr = m.ToArray();
        };
        int p = Enum.GetValues(typeof(ChunkNHMapCSManager.Instr.MODIFICATION)).Length;
        int res = 0;
        for (int i = MAX_INSTR; i < MAX_INSTR; i--)
        {
            res += (int)instr[i] * (int)Mathf.Pow(p, i);
        }
        return res;
    }
}
public class ChunkNHMapCSManager
{

    private ComputeShader cs;


    //NOTE one per per planet ! (same noise)
    public ChunkNHMapCSManager(ComputeShader cs)
    {
        this.cs = cs;
    }

    struct float3
    {
        public float x;
        public float y;
        public float z;
        public Vector3 ToVec()
        {
            return new Vector3(x, y, z);
        }
        public float3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }

    public void GenMap(RenderTexture buffer, Vector3[] v, Vector3[] n, Color[] c, Vector3 origin, Vector3 mx, Vector3 my, float gRad, float scale, float multiplier, Vector3 offset, Biome[] biomes)
    {
        // VertexUpdate


        //TODO add biome data transfer !
        
        


        ComputeBuffer vBuff = new ComputeBuffer(v.Length, sizeof(float)*3);
        ComputeBuffer nBuff = new ComputeBuffer(n.Length, sizeof(float)*3);
        ComputeBuffer cBuff = new ComputeBuffer(c.Length, sizeof(float)*4);
        ComputeBuffer bMinPredsBuff = new ComputeBuffer(biomes.Length, sizeof(float)*4);
        ComputeBuffer bMaxPredsBuff = new ComputeBuffer(biomes.Length, sizeof(float)*4);
        ComputeBuffer genToUseBuff = new ComputeBuffer(biomes.Length, sizeof(int));
        ComputeBuffer paramsOfGenBuff = new ComputeBuffer(biomes.Length, sizeof(float)*16);
        ComputeBuffer bBlendBuff = new ComputeBuffer(biomes.Length, sizeof(float));




        float3[] vA = new float3[v.Length];
        float3[] nA = new float3[n.Length];
        float4[] cA = new float4[c.Length];
        float4[] bMinPreds = new float4[biomes.Length];
        float4[] bMaxPreds = new float4[biomes.Length];
        int[] bGen = new int[biomes.Length];
        float4x4[] bGenP = new float4x4[biomes.Length];
        float[] bBlend = new float[biomes.Length];

        for (int i = 0; i < v.Length; i++)
        {
            vA[i] = new(){x=v[i].x, y=v[i].y, z=v[i].z};
            nA[i] = new(){x=n[i].x, y=n[i].y, z=n[i].z};
            cA[i] = new float4(c[i].r * 2f - 1f, c[i].g * 2f - 1f, c[i].b * 2f - 1f, c[i].a * 2f - 1f);
        };

        for (int i = 0; i < biomes.Length; i++)
        {
            bMinPreds[i] = biomes[i].GetMinPreds();
            bMaxPreds[i] = biomes[i].GetMaxPreds();
            bGen[i] = biomes[i].GetGenToUse();
            bGenP[i] = biomes[i].GetGenParams();
            bBlend[i] = biomes[i].blendingFactor;
        }

        vBuff.SetData(vA);
        nBuff.SetData(nA);
        bMinPredsBuff.SetData(bMinPreds);
        bMaxPredsBuff.SetData(bMaxPreds);
        genToUseBuff.SetData(bGen);
        paramsOfGenBuff.SetData(bGenP);
        bBlendBuff.SetData(bBlend);

        cs.SetTexture(0, "_NHMap", buffer);

        cs.SetBuffer(1, "_vertices", vBuff);
        cs.SetBuffer(1, "_normals", nBuff);

        cs.SetBuffer(1, "_color", cBuff);

        cs.SetInt("_vNum", v.Length);
        cs.SetFloat("_bRad", gRad);

        cs.SetVector("_origin", origin);
        cs.SetVector("_mx", mx);
        cs.SetVector("_my", my);

        cs.SetFloat("_scale", scale);
        cs.SetFloat("_multiplier", multiplier);
        cs.SetVector("_offset", new Vector4(offset.x, offset.y, offset.z, 0));


        cs.SetInt("_numBiomes", biomes.Length);

        cs.SetBuffer(1, "_minPredicates", bMinPredsBuff);
        cs.SetBuffer(1, "_maxPredicates", bMaxPredsBuff);
        cs.SetBuffer(1, "_genToUse", genToUseBuff);
        cs.SetBuffer(1, "_paramsOfGen", paramsOfGenBuff);
        cs.SetBuffer(1, "_blendingFactor", bBlendBuff);
    


        cs.Dispatch(0, buffer.width/8, buffer.height/8, 1);
        
        cs.Dispatch(1, v.Length, 1, 1);

        vBuff.GetData(vA);
        nBuff.GetData(nA);
        cBuff.GetData(cA);
        for (int i = 0; i < vA.Length; i++) 
        {
            v[i] = vA[i].ToVec();
            n[i] = nA[i].ToVec();
            c[i] = new Color(cA[i].x, cA[i].y, cA[i].z, cA[i].w);
            
        }
        vBuff.Release();
        nBuff.Release();
        cBuff.Release();
        bMinPredsBuff.Release();
        bMaxPredsBuff.Release();
        genToUseBuff.Release();
        paramsOfGenBuff.Release();
        bBlendBuff.Release();
    }

    [Serializable]
    public class Instr
    {
        public enum MODIFICATION
        {
            NONE,
        }
        [SerializeField]
        private MODIFICATION instr;
        [SerializeField]
        private Vector4 arg1;
        [SerializeField]
        private Vector4 arg2;
        public Vector4[] args => new Vector4[]{arg1, arg2};

        public MODIFICATION GetInstr() => instr;

    }

}