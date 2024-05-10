﻿namespace LionFire.Trading.Automation.Bots;

public class PUnidirectionalBot
{
    #region Long vs Short

    [Parameter("Long trades only if true, otherwise short only", DefaultValue = false)]
    public bool Long { get; set; }
    public bool Short => !Long;

    #endregion

    #region Open vs Close 

    [Parameter("Reverse open and close operations", DefaultValue = false, DefaultMin = false, DefaultMax = true)]
    public bool ReverseOpenClose { get; set; }

    #endregion
}
