using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class ActorTarget : MonoBehaviour
    {
        [SerializeField] private Actor _master;
        [SerializeField] private Collider _col;

        public Actor master => _master;
        public Collider col => _col;

        public void Awake()
        {
            _master ??= GetComponentInParent(typeof(Actor)) as Actor;
            _col ??= GetComponent(typeof(Collider)) as Collider;
        }
    }
}