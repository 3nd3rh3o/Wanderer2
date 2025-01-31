using System;
using System.Dynamic;
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
            switch (type)
            {
                case ChunkTaskTYPE.Split:
                    chunk.Split();
                    return;
                case ChunkTaskTYPE.UnSplit:
                    chunk.UnSplit();
                    return;
            };
        }
    }

    public enum ChunkTaskTYPE
    {
        Split,
        UnSplit
        
    }
}