﻿using LionFire.Instantiating;
using System.Threading.Channels;

namespace LionFire.Trading; // TODO: Move to .Components or .Slots namespace

public class InputSlot : Slot
{
}

public interface IInputParameters
{
    int Phase { get; }
}

#if UNUSED
public class InputParameters<T> : IInputParameters
{
    public required T Parameters { get; init; }
    public int Phase { get; init; }
}
#endif



