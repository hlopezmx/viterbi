
namespace ViterbiTracking
{
    /// <summary>
    /// Base structure for the tracked object. 
    /// It can be extended or inherited as needed
    /// </summary>
    class TrackedObject
    {
        private int _x;
        private int _y;
        public int cost;
        public TrackedObject closestSource;
        public bool taken = false;
        public int _ith;

        public TrackedObject(int x, int y)
        {
            _x = x;
            _y = y;
        }
        public TrackedObject(int x, int y, int ith)
        {
            _x = x;
            _y = y;
            _ith = ith;
        }
        
        public int x
        {
            get { return _x; }
        }

        public int y
        {
            get { return _y; }
        }
        public int ith
        {
            get { return _ith; }
        }

    }
}
