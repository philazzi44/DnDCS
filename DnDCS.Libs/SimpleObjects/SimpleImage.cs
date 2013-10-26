using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DnDCS.Libs.SimpleObjects
{
    public class SimpleImage
    {
        public byte[] Bytes { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public SimpleImage(int width, int height, byte[] bytes)
        {
            Bytes = bytes;
            Width = width;
            Height = height;
        }
    }
}
