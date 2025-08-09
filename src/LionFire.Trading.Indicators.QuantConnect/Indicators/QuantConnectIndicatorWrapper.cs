
namespace LionFire.Trading.Indicators.QuantConnect_;

public abstract class QuantConnectIndicatorWrapper<TConcrete, TQuantConnectIndicator, TParameters, TInput, TOutput> : SingleInputIndicatorBase<TConcrete, TParameters, TInput, TOutput>
    where TConcrete : IndicatorBase<TConcrete, TParameters, TInput, TOutput>, IIndicator2<TConcrete, TParameters, TInput, TOutput>
{
    #region (static)

    static QuantConnectIndicatorWrapper()
    {
        // OPTIMIZE - https://stackoverflow.com/a/3344181/208304
        if (typeof(TOutput) == typeof(double)) { ConvertToOutput = (v) => (TOutput)(object)Convert.ToDouble(v); }
        else if (typeof(TOutput) == typeof(float)) { ConvertToOutput = (v) => (TOutput)(object)Convert.ToSingle(v); }
        else if (typeof(TOutput) == typeof(decimal)) { ConvertToOutput = (v) => (TOutput)(object)v; }
        else ConvertToOutput = _ => throw new NotSupportedException($"Not implemented: conversion from decimal to {typeof(TOutput).FullName}");
    }

    public static Func<decimal, TOutput> ConvertToOutput;

    #endregion

    public QuantConnectIndicatorWrapper(TQuantConnectIndicator quantConnectIndicator)
    {
        WrappedIndicator = quantConnectIndicator;
    }

    #region State

    public TQuantConnectIndicator WrappedIndicator { get; protected set; }

    #endregion

    public DateTime LastEndTime { get; protected set; }
}

