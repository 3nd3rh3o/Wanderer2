using System;
using System.Numerics;

public class ChunkTask
{
    public enum TYPE
    {
        ADDCHILD,
        KILLCHILD
    }
    public TYPE type;
    public Chunk chunk;
    public ChunkTask(TYPE type, Chunk chunk)
    {
        this.type = type;
        this.chunk = chunk;
    }
}