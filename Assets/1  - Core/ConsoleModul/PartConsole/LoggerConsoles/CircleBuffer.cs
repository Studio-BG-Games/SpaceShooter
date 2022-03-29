namespace ConsoleModul.PartConsole.LoggerConsoles
{
    public class CircleBuffer<T>
    {
        private int _size;
        private int _head;
        private T[] _elements;
        
        
        public CircleBuffer(int size)
        {
            _size = size;
            _elements = new T[size];
            _head = 0;
        }

        public void Add(T element)
        {
            _elements[_head] = element;
            _head++;
            _head = _head % _size;
        }

        public T[] GetElements()
        {
            T[] result = new T[_size];
            for (int i = 0; i < _size; i++) result[i] = _elements[(_head + i + 1) % _size];
            return result;
        }

        public void ChangeSize(int newSize)
        {
            _size = newSize;
            Clear();
        }
        
        public void Clear()
        {
            _elements = new T[_size];
            _head = 0;
        }
    }
}