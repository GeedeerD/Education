namespace Models
{
    public struct Point3DPtinter
    {
        public Point3DPtinter()
        {
            
        }
        public Point3DPtinter(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
