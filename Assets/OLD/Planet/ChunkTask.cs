#if !UNITY_EDITOR

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
#endif