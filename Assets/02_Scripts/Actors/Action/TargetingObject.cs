using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    public class TargetingObject : MonoBehaviour
    {
        public LinkedListNode<TargetingObject> Node;
        private ActorManager.TargetingObjectPool _pool;

        private Actor _master;
        private System.Action<ActorTarget> _hitCallback;

        private float _fDelayTime = 0f;
        private float _fLifeTime = 0f;
        
        private HashSet<Actor> _setAlreadyTargeting = new();
        
        private void Awake()
        {
            Node = new LinkedListNode<TargetingObject>(this);
        }

        private void Update()
        {
            if (_fDelayTime > 0f)
            {
                _fDelayTime -= Time.deltaTime * _master.TimeCtrl.TotalTimeScale;
                if (_fDelayTime <= 0f)
                    SetActive_Children(true);
            }
            else if (_fLifeTime > 0f)
            {
                _fLifeTime -= Time.deltaTime * _master.TimeCtrl.TotalTimeScale;
                if (_fLifeTime <= 0f)
                    ReturnToPool();
            }
        }

        private void OnDisable()
        {
            _master = null;
            _hitCallback = null;
            _setAlreadyTargeting.Clear();
            
            SetActive_Children(false);
        }

        private void OnDestroy()
        {
            Node.RemoveSelf();
            Node = null;
            _pool = null;

            _setAlreadyTargeting = null;
        }

        public void Init(Actor master, float fDelayTime, float fLifeTime, Action<ActorTarget> hitCallback)
        {
            _master = master;
            _hitCallback = hitCallback;

            _fDelayTime = fDelayTime;
            _fLifeTime = fLifeTime;

            // 딜레이가 있다면 자식 오브젝트를 꺼둔다
            SetActive_Children(_fDelayTime <= float.Epsilon);
        }
        
        public void ReturnToPool()
        {
            _master = null;
            Node.RemoveSelf();

            if (Managers.IsValid && _pool != null)
                _pool.ReleaseTargetingObject(this);
            else
                Destroy(gameObject);
        }

        public void SetActive_Children(bool bActive)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var trChild = this.transform.GetChild(i);
                trChild.gameObject.SetActive(bActive);
            }
        }

        public void OnTriggerEnterByChilden(Collider col)
        {
            _hitCallback?.Invoke(col.GetComponent(typeof(ActorTarget)) as ActorTarget);
        }

        #if UNITY_EDITOR
        public static readonly Color s_gizmoColor = new Color(0f, 0f, 1f, 0.5f);
        public static readonly Color s_gizmoColor_EventItem = new(0f, 1f, 0f, 0.5f);
        public static readonly Color s_gizmoColor_DelayedEventItem = new (1f, 0f, 0f, 0.5f);
        public static readonly Color s_gizmoColor_SelectedEventItem = new(1f, 200f / 255f * 0.75f, 0f, 0.5f);

        private Color _gizmoColor = s_gizmoColor;
        public void OnEditor_SetGizmoColor(Color color)
        {
            _gizmoColor = color;
        }
        
        private void OnDrawGizmos()
        {
            var defaultGizmoColor = Gizmos.color;
            Gizmos.color = Color.yellow;

            Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);
            
            Gizmos.color = _gizmoColor;
            {
                var childCols = GetComponentsInChildren<Collider>();
                foreach (var childCol in childCols)
                {
                    Gizmos.matrix = childCol.transform.localToWorldMatrix;

                    switch (childCol)
                    {
                        case BoxCollider boxCol:
                        {
                            var size = boxCol.size;
                            Gizmos.DrawCube(Vector3.zero, new(size.x, size.y, size.z));
                        }
                            break;
                        case SphereCollider sphereCol:
                        {
                            Gizmos.DrawSphere(Vector3.zero, sphereCol.radius);
                        }
                            break;
                        case CapsuleCollider capsuleCol:
                        {
                            if (capsuleCol.radius * 2f >= capsuleCol.height)
                            {
                                Gizmos.DrawSphere(Vector3.zero, capsuleCol.radius);
                            }
                            else
                            {
                                var fRadius = capsuleCol.radius;
                                var fHeight = capsuleCol.height;

                                var vSphereCenter = new Vector3(0f, fHeight * 0.5f - (fRadius), 0f);

                                Gizmos.DrawSphere(vSphereCenter, fRadius);
                                Gizmos.DrawSphere(-vSphereCenter, fRadius);

                                var vTF = new Vector3(0f, vSphereCenter.y, -fRadius);
                                var vTB = new Vector3(0f, vSphereCenter.y, fRadius);
                                var vTL = new Vector3(-fRadius, vSphereCenter.y, 0f);
                                var vTR = new Vector3(fRadius, vSphereCenter.y, 0f);

                                var vBF = new Vector3(0f, -vSphereCenter.y, -fRadius);
                                var vBB = new Vector3(0f, -vSphereCenter.y, fRadius);
                                var vBL = new Vector3(-fRadius, -vSphereCenter.y, 0f);
                                var vBR = new Vector3(fRadius, -vSphereCenter.y, 0f);

                                Gizmos.DrawLine(vTF, vBF);
                                Gizmos.DrawLine(vTB, vBB);
                                Gizmos.DrawLine(vTL, vBL);
                                Gizmos.DrawLine(vTR, vBR);
                            }
                        }
                            break;
                        case MeshCollider meshCol:
                        {
                            if (meshCol.sharedMesh != null)
                                Gizmos.DrawMesh(meshCol.sharedMesh);
                        }
                            break;
                    }
                }
            }
            Gizmos.color = defaultGizmoColor;
        }
        #endif
    }    
}