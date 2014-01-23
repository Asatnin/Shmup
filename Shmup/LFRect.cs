using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shmup
{
    class LFRect
    {
        public float x;
        public float y;
        public float w;
        public float h;

        public LFRect() { }

        public LFRect(LFRect rect)
        {
            this.x = rect.x;
            this.y = rect.y;
            this.w = rect.w;
            this.h = rect.h;
        }
    }
}