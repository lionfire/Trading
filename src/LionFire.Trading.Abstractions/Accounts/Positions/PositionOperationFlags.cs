namespace LionFire.Trading;

[Flags]
public enum PositionOperationFlags
{
    Unspecified = 0,

    /// <summary>
    /// If ResizeExistingPosition is set, and a compatible existing position is found, it will be grown, instead of opening a new position.
    /// </summary>
    ResizeExistingPosition = 1 << 0,

    /// <summary>
    /// Allows new positions to be created.
    /// If ResizeExistingPosition is set, and a compatible existing position is found, it will be grown, instead of opening a new position.
    /// If ResizeExistingPosition is not set, or no compatible existing position is found, and AllowNewPosition is set, a new position will be opened.
    /// </summary>
    AllowNewPosition = 1 << 1,

    ManipulateOppositeDirectionPositions = 1 << 2,

    AllowCloseAndOpenAtOnce = 1 << 3, // Default: disabled

    //ReplaceSameDirection = 1 << 4, 
    //ReplaceSameAndOppositeDirection = 1 << 5,

    Open = 1 << 6,

    Close = 1 << 8,
    CloseOnly = 1 << 9,

    Default = ResizeExistingPosition | AllowNewPosition | ManipulateOppositeDirectionPositions,
}


