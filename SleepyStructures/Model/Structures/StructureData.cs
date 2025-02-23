using System.Collections.Generic;

namespace SleepyStructures.Model.Structures;

public class StructureData
{
    public int IdleTicks;
    public bool Sleeping;
    public readonly List<WearNTearTracked> Pieces = new();
}
