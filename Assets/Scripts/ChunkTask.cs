using System;
using System.Numerics;

public struct ChunkTask
{
    public Action action;
    public Chunk chunk;
    public Vector3 center;
    public int DIR;
    public int LOD;
    public float size;
    public float gRad;
}