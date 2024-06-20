using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Actor
{
    public class TargetingObjectChild : MonoBehaviour
    {
        [FormerlySerializedAs("_root")] [SerializeField]
        private TargetingObject _parent;
        [SerializeField]
        protected Collider _col;
        [SerializeField]
        protected Rigidbody _rigid;

        protected virtual void Awake()
        {
            if (_parent == null)
                _parent = gameObject.GetComponentInParent(typeof(TargetingObject)) as TargetingObject;
            
            if (_rigid == null)
                _rigid = gameObject.AddComponent(typeof(Rigidbody)) as Rigidbody;

            _rigid.isKinematic = true;
            _rigid.useGravity = false;
            _rigid.drag = 0f;
            _rigid.mass = 1f;

            if (_col == null)
                _col = gameObject.GetComponent(typeof(Collider)) as Collider;
            
            if (_col != null)
                _col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            _parent.OnTriggerEnterByChilden(other);
        }

        public void RefreshComponent()
        {
            _parent = gameObject.GetComponentInParent(typeof(TargetingObject)) as TargetingObject;
            _rigid = gameObject.GetOrAddComponent<Rigidbody>();
            _col = gameObject.GetComponent(typeof(Collider)) as Collider;
            
            _rigid.isKinematic = true;
            _rigid.useGravity = false;
            _rigid.drag = 0f;
            _rigid.mass = 1f;
            
            if (_col != null)
                _col.isTrigger = true;
            else
                Debug.LogError("TargetingObjectChild :: Not Found Collider");
        }
    }
}