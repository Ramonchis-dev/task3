﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace task3
{
    public class Dice(int[] faces)
    {
        public int[] Faces { get; } = faces;
        public override string ToString() => $"[{string.Join(",", Faces)}]";
    }
}
