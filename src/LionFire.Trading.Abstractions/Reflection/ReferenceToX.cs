namespace LionFire.Trading.DataFlow;

public static class ReferenceToX
{
    public static Type? GetValueType(this Type type)
    {
        return IReferenceToX.TryGetTypeOfReferenced(type);
    }
}
