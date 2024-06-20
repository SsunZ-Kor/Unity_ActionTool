using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class TimeController
    {
        public Actor Master { get; private set; }
        
        public float TotalTimeScale { get; private set; } = 1f;

        public TimeController(Actor master)
        {
            Master = master;
        }
    }
}