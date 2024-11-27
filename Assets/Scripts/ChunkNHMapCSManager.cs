using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    private int instr;
    private Vector4[] args;


    //NOTE one per per planet ! (same noise)
    public ChunkNHMapCSManager(ComputeShader cs, Instr[] instructions)
    {
        instr = instructions.UnPackInstr();
        args = instructions.UnPackArgs();
        this.cs = cs;
    }

    public void UpdateSettings(Instr[] instructions)
    {
        instr = instructions.UnPackInstr();
        args = instructions.UnPackArgs();
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
    }

    public void GenMap(RenderTexture buffer, Vector3[] v, Vector3[] n, Vector3 origin, Vector3 mx, Vector3 my)
    {
        cs.SetTexture(0, "NHMap", buffer);
        cs.SetMatrix("arg1", new Matrix4x4(args[0], args[1], args[2], args[3]).transpose);
        cs.SetMatrix("arg2", new Matrix4x4(args[4], args[5], args[6], args[7]).transpose);
        cs.SetVector("origin", origin);
        cs.SetVector("mx", mx);
        cs.SetVector("my", my);

        ComputeBuffer vBuff = new ComputeBuffer(v.Length, sizeof(float)*3);
        ComputeBuffer nBuff = new ComputeBuffer(n.Length, sizeof(float)*3);

        float3[] vA = new float3[v.Length];
        float3[] nA = new float3[n.Length];

        for (int i = 0; i < v.Length; i++)
        {
            vA[i] = new(){x=v[i].x, y=v[i].y, z=v[i].z};
            nA[i] = new(){x=n[i].x, y=n[i].y, z=n[i].z};
        };

        vBuff.SetData(vA);
        nBuff.SetData(nA);
        cs.SetBuffer(0, "vertices", vBuff);
        cs.SetBuffer(0, "normals", nBuff);
        cs.Dispatch(0, buffer.width/8, buffer.height/8, 1);

        vBuff.GetData(vA);
        nBuff.GetData(nA);
        for (int i = 0; i < vA.Length; i++) 
        {
            v[i] = vA[i].ToVec();
            n[i] = nA[i].ToVec();
        }
        vBuff.Release();
        nBuff.Release();
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