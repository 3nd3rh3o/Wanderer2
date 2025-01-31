using System;
using static Wanderer.TeluricGenerator;

namespace Wanderer
{
    public class ChunkTask
    {
        public ChunkTaskTYPE type;
        public Chunk chunk;
        public ChunkTask(ChunkTaskTYPE type, Chunk chunk)
        {
            this.type = type;
            this.chunk = chunk;
        }

        internal void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public enum ChunkTaskTYPE
    {
        Split,
        UnSplit
        
    }
}