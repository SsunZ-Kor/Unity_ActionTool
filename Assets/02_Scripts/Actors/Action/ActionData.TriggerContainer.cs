using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    public partial class ActionData
    {
        /* Tag :: Define New Trigger :: Define Container  */
        [SerializeField] public List<ActionTrigger_OnArea> listTriggersOnArea = new();
        [SerializeField] public List<ActionTrigger_OnInput> listTriggersOnInput = new();
        [SerializeField] public List<ActionTrigger_OnEndAction> listTriggersOnEndAction = new();

        private void _OnAfterDeserialize_ActionTrigger()
        {
            _Sort_ListActionTrigger(listTriggersOnArea);
            _Sort_ListActionTrigger(listTriggersOnInput);
        }

#if UNITY_EDITOR
        public ActionTriggerBase Editor_AddActionTriggerData(ActionTriggerBase triggerData)
        {
            if (triggerData == null)
                return null;
            
            switch (triggerData)
            {
                /* Tag :: Define New Trigger :: Add Container  */
                case ActionTrigger_OnArea triggerOnArea:
                    listTriggersOnArea.Add(triggerOnArea);
                    _Sort_ListActionTrigger(listTriggersOnArea);
                    break;
                case ActionTrigger_OnInput triggerOnInput:
                    listTriggersOnInput.Add(triggerOnInput);
                    _Sort_ListActionTrigger(listTriggersOnInput);
                    break;
                case ActionTrigger_OnEndAction triggerOnEndAction:
                    listTriggersOnEndAction.Add(triggerOnEndAction);
                    break;
            }

            EditorUtility.SetDirty(this);
            return triggerData;
        }
        
        public void Editor_RemoveActionTrigger(ActionTriggerBase triggerData)
        {
            switch (triggerData)
            {
                /* Tag :: Define New Trigger :: Remove Container  */
                case ActionTrigger_OnInput     triggerData_OnInput    : listTriggersOnInput    .Remove(triggerData_OnInput);     break;
                case ActionTrigger_OnArea      triggerData_OnArea     : listTriggersOnArea     .Remove(triggerData_OnArea);      break;
                case ActionTrigger_OnEndAction triggerData_OnEndAction: listTriggersOnEndAction.Remove(triggerData_OnEndAction); break;
            }
            
            EditorUtility.SetDirty(this);
        }
        
        public void Editor_ShowCreateTriggerMenu(System.Action<ActionTriggerBase> SelectContextCallback)
        {
            void _onSelectMenu(Type T) => SelectContextCallback?.Invoke(Editor_CreateActionTriggerData(T));
            
            var menu = new GenericMenu();

            /* Tag :: Define New Trigger :: Add AddTriggerMenu  */
            menu.AddItem(new GUIContent("OnInput"), false, () => _onSelectMenu(typeof(ActionTrigger_OnInput)));
            menu.AddItem(new GUIContent("InArea" ), false, () => _onSelectMenu(typeof(ActionTrigger_OnArea )));

            menu.ShowAsContext();
        }
        
        public ActionTriggerBase Editor_CreateActionTriggerData(System.Type triggerType)
        {
            if (triggerType.IsAbstract || !triggerType.IsSubclassOf(typeof(ActionTriggerBase)))
                return null;

            var triggerData = Activator.CreateInstance(triggerType) as ActionTriggerBase;
            if (triggerData is ActionTriggerDurationBase triggerDurationData)
                triggerDurationData.Editor_EndFrame = Frame;

            return Editor_AddActionTriggerData(triggerData);
        }


        public static string Editor_GetTriggerDisplayTypeName(System.Type type)
        {
            return type.Name.Replace("ActionTrigger_", "");
        }

        private void _Sort_ListActionTrigger<T>(List<T> list) where T : ActionTriggerDurationBase
        {
            list.Sort((l, r) =>
            {
                var result = l.startTime.CompareTo(r.startTime);
                if (result != 0)
                    return result;

                result = l.HasEndTime.CompareTo(r.HasEndTime);
                if (result != 0 || l.HasEndTime == false)
                    return result;

                result = l.endTime.CompareTo(r.endTime);
                return result;
            });
        }
#endif
    }
}