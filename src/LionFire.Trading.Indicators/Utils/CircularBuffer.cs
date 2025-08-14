namespace LionFire.Trading.Indicators.Utils;

/// <summary>
/// Custom CircularBuffer implementation for efficient sliding window calculations
/// </summary>
public class CircularBuffer<T>
{
    private readonly T[] buffer;
    private readonly int capacity;
    private int head = 0;
    private int count = 0;

    public CircularBuffer(int capacity)
    {
        this.capacity = capacity;
        buffer = new T[capacity];
    }

    public int Count => count;

    /// <summary>
    /// Gets the oldest item in the buffer (front of the queue)
    /// </summary>
    public T Front
    {
        get
        {
            if (count == 0)
                throw new InvalidOperationException("Buffer is empty");
            
            var index = (head - count + capacity) % capacity;
            return buffer[index];
        }
    }

    /// <summary>
    /// Gets the most recent item in the buffer (back of the queue)
    /// </summary>
    public T Back
    {
        get
        {
            if (count == 0)
                throw new InvalidOperationException("Buffer is empty");
            
            var index = (head - 1 + capacity) % capacity;
            return buffer[index];
        }
    }

    /// <summary>
    /// Indexer to access buffer elements by position
    /// Index 0 is the oldest element, Index Count-1 is the newest element
    /// </summary>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            var actualIndex = (head - count + index + capacity) % capacity;
            return buffer[actualIndex];
        }
    }

    /// <summary>
    /// Adds an item to the buffer (same as PushBack)
    /// </summary>
    public void Add(T item)
    {
        buffer[head] = item;
        head = (head + 1) % capacity;
        if (count < capacity)
            count++;
    }

    /// <summary>
    /// Alias for Add method for compatibility
    /// </summary>
    public void PushBack(T item) => Add(item);

    /// <summary>
    /// Gets the size of the buffer (alias for Count)
    /// </summary>
    public int Size => count;

    public void Clear()
    {
        head = 0;
        count = 0;
        Array.Clear(buffer, 0, buffer.Length);
    }
}