using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif


namespace Actor
{
    public partial class Actor : MonoBehaviour
    {
        [SerializeField] 
        private CharacterController _charCtrl;
        
        public BrainController BrainCtrl { get; private set; }
        public AnimController AnimCtrl { get; private set; }
        public ActionController ActionCtrl { get; private set; }
        public MoveController MoveCtrl { get; private set; }
        public ModelController ModelCtrl { get; private set; }
        public TimeController TimeCtrl { get; private set; }


        private void Awake()
        {
            TimeCtrl = new(this);
            BrainCtrl  = new(this);
            ActionCtrl = new(this);
            AnimCtrl   = new(this);
            MoveCtrl   = new(this, _charCtrl);
        }

        private void OnEnable()
        {
            MoveCtrl.RefreshGroundInfo();
        }

        private void Update()
        {
            BrainCtrl.OnActor_Update();
            ActionCtrl.OnActor_Update(Time.deltaTime); // Todo :: Time Ctrl
            MoveCtrl.OnActor_Update(Time.deltaTime);
            AnimCtrl.OnActor_OnUpdate();
        }

        private void FixedUpdate()
        {
            ActionCtrl.OnActor_FixedUpdate(Time.fixedDeltaTime);
            MoveCtrl.OnActor_FixedUpdate(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            ActionCtrl.OnActor_Destroy();

            BrainCtrl = null;
            AnimCtrl = null;
            ActionCtrl = null;
            MoveCtrl = null;
            ModelCtrl = null;
        }

        public void AttachModel(ModelController modelCtrl)
        {
            ModelCtrl = modelCtrl;
            modelCtrl.transform.SetParent(this.transform);
            modelCtrl.transform.Reset();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            MoveCtrl.OnActor_OnControllerColliderHit(hit);
        }
        
#if UNITY_EDITOR
        [MenuItem ("CONTEXT/AnimatorOverrideController/Set Actor AnimationClip")]
        static void DoubleMass (MenuCommand command) 
        {
            var animCtrl = command.context as AnimatorOverrideController;
            if (animCtrl == null)
                return;

            var id = animCtrl.name.Replace("animCtrl_", "");

            /* 폴더 경로 찾기 */
            var path = AssetDatabase.GetAssetPath(animCtrl);
            var folder = Path.GetDirectoryName(path);
            if (folder == null)
                return;
            
            folder = folder.Replace('\\', '/');
            
            /* AnimClip Load */
            var animClipGUIDs = AssetDatabase.FindAssets("t:AnimationClip", new[] { folder });
            foreach (var animClipGUID in animClipGUIDs)
            {
                var animClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(animClipGUID));
                if (animClip == null)
                    continue;

                var stateName = animClip.name.Replace($"animClip_{id}_", "");
                try
                {
                    animCtrl[stateName] = animClip;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
                
                EditorUtility.SetDirty(animClip);
            }
            
            Debug.Log("Done :: Set Actor AnimationClip");
        }
#endif
    }
}