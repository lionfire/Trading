using CircularBuffer;

namespace LionFire.Trading.ValueWindows;

public static class CircularBufferX
{
    public static T[] ToReversedArray<T>(this CircularBuffer<T> @this)
    {
        T[] array = new T[@this.Size];
        int num = 0;
        for (int i = @this.Size - 1; i >= 0; i--)
        {
            array[num++] = @this[i];
        }
        return array;
    }
}
