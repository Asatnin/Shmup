struct BoundingRectangle
{
    float x, y, width, height;

    public BoundingRectangle(float x, float y, float width, float height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }

    public float Left
    {
        get
        {
            return x;
        }
    }

    public float Right
    {
        get
        {
            return x + width;
        }
    }

    public float Top
    {
        get
        {
            return y;
        }
    }

    public float Bottom
    {
        get
        {
            return y + height;
        }
    }
}