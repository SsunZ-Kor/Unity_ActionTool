using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actor
{
    public partial class ActionData
    {
        /* Tag :: Define New Event :: Define Container */
        [SerializeField] private List<ActionEventData_Animation              > _listAnimation                   = new();
        [SerializeField] private List<ActionEventData_IgnoreGravity          > _listIgnoreGravitie              = new();
        
        [SerializeField] private List<ActionEventData_MovePos_ToForward      > _listMovePos_ToForward           = new();
        [SerializeField] private List<ActionEventData_MovePos_ToInput        > _listMovePos_ToInput             = new();
        [SerializeField] private List<ActionEventData_MovePos_ToBrainTarget  > _listMovePos_ToBrainTarget       = new();
        
        [SerializeField] private List<ActionEventData_MoveVel_ToForward      > _listMoveVel_ToForward           = new();
        [SerializeField] private List<ActionEventData_MoveVel_ToInput        > _listMoveVel_ToInput             = new();
        [SerializeField] private List<ActionEventData_MoveVel_ToBrainTarget  > _listMoveVel_ToBrainTarget       = new();
        
        [SerializeField] private List<ActionEventData_RotateAngle_Left       > _listRotateAngle_Left            = new();
        [SerializeField] private List<ActionEventData_RotateAngle_Right      > _listRotateAngle_Right           = new();
        
        [SerializeField] private List<ActionEventData_RotateDir_ToInput      > _listRotateDir_ToInput           = new();
        [SerializeField] private List<ActionEventData_RotateDir_ToBrainTarget> _listRotateDir_ToBrainTarget     = new();
        [SerializeField] private List<ActionEventData_RotateDir_ToMove       > _listRotateDir_ToMove            = new();

        [SerializeField] private List<ActionEventData_Hit                    > _listHit                         = new();
        
        [SerializeField] private List<ActionEventData_FX                     > _listFX                          = new();
        [SerializeField] private List<ActionEventData_SFX                    > _listSFX                         = new();
        
        
        private void _OnAfterDeserialize_ActionEvent()
        {
            listEventData.Clear();

            /* Tag :: Define New Event :: Add Whole Data  */
            foreach (var eventData in _listAnimation              ) listEventData.Add(eventData);
            foreach (var eventData in _listIgnoreGravitie         ) listEventData.Add(eventData);
            foreach (var eventData in _listMovePos_ToForward      ) listEventData.Add(eventData);
            foreach (var eventData in _listMovePos_ToInput        ) listEventData.Add(eventData);
            foreach (var eventData in _listMovePos_ToBrainTarget  ) listEventData.Add(eventData);
            foreach (var eventData in _listMoveVel_ToForward      ) listEventData.Add(eventData);
            foreach (var eventData in _listMoveVel_ToInput        ) listEventData.Add(eventData);
            foreach (var eventData in _listMoveVel_ToBrainTarget  ) listEventData.Add(eventData);
            foreach (var eventData in _listRotateAngle_Left       ) listEventData.Add(eventData);
            foreach (var eventData in _listRotateAngle_Right      ) listEventData.Add(eventData);
            foreach (var eventData in _listRotateDir_ToInput      ) listEventData.Add(eventData);
            foreach (var eventData in _listRotateDir_ToBrainTarget) listEventData.Add(eventData);
            foreach (var eventData in _listRotateDir_ToMove       ) listEventData.Add(eventData);
            foreach (var eventData in _listHit                    ) listEventData.Add(eventData);
            foreach (var eventData in _listFX                     ) listEventData.Add(eventData);
            foreach (var eventData in _listSFX                    ) listEventData.Add(eventData);

            _Sort_ListActionEvent(listEventData);
        }
        
        #if UNITY_EDITOR
        
        public ActionEventDataBase Editor_AddActionEventData(ActionEventDataBase eventData)
        {
            if (eventData == null)
                return null;
            
            void _AddEndSort<T>(T eventData, List<T> list) where T : ActionEventDataBase
            {
                list.Add(eventData);
                _Sort_ListActionEvent(list);
            }
            
            switch (eventData)
            {
                /* Tag :: Define New Event :: Add Container  */
                case ActionEventData_Animation               eventData_Animation              : _AddEndSort(eventData_Animation              , _listAnimation              ); break;  
                case ActionEventData_IgnoreGravity           eventData_IgnoreGravity          : _AddEndSort(eventData_IgnoreGravity          , _listIgnoreGravitie         ); break; 
                case ActionEventData_MovePos_ToForward       eventData_MovePos_ToForward      : _AddEndSort(eventData_MovePos_ToForward      , _listMovePos_ToForward      ); break; 
                case ActionEventData_MovePos_ToInput         eventData_MovePos_ToInput        : _AddEndSort(eventData_MovePos_ToInput        , _listMovePos_ToInput        ); break; 
                case ActionEventData_MovePos_ToBrainTarget   eventData_MovePos_ToBrainTarget  : _AddEndSort(eventData_MovePos_ToBrainTarget  , _listMovePos_ToBrainTarget  ); break; 
                case ActionEventData_MoveVel_ToForward       eventData_MoveVel_ToForward      : _AddEndSort(eventData_MoveVel_ToForward      , _listMoveVel_ToForward      ); break; 
                case ActionEventData_MoveVel_ToInput         eventData_MoveVel_ToInput        : _AddEndSort(eventData_MoveVel_ToInput        , _listMoveVel_ToInput        ); break; 
                case ActionEventData_MoveVel_ToBrainTarget   eventData_MoveVel_ToBrainTarget  : _AddEndSort(eventData_MoveVel_ToBrainTarget  , _listMoveVel_ToBrainTarget  ); break; 
                case ActionEventData_RotateAngle_Left        eventData_RotateAngle_Left       : _AddEndSort(eventData_RotateAngle_Left       , _listRotateAngle_Left       ); break; 
                case ActionEventData_RotateAngle_Right       eventData_RotateAngle_Right      : _AddEndSort(eventData_RotateAngle_Right      , _listRotateAngle_Right      ); break; 
                case ActionEventData_RotateDir_ToInput       eventData_RotateDir_ToInput      : _AddEndSort(eventData_RotateDir_ToInput      , _listRotateDir_ToInput      ); break; 
                case ActionEventData_RotateDir_ToBrainTarget eventData_RotateDir_ToBrainTarget: _AddEndSort(eventData_RotateDir_ToBrainTarget, _listRotateDir_ToBrainTarget); break; 
                case ActionEventData_RotateDir_ToMove        eventData_RotateDir_ToMove       : _AddEndSort(eventData_RotateDir_ToMove       , _listRotateDir_ToMove       ); break;
                case ActionEventData_Hit                     eventData_Hit                    : _AddEndSort(eventData_Hit                    , _listHit                    ); break; 
                case ActionEventData_FX                      eventData_FX                     : _AddEndSort(eventData_FX                     , _listFX                     ); break; 
                case ActionEventData_SFX                     eventData_SFX                    : _AddEndSort(eventData_SFX                    , _listSFX                    ); break; 
            }

            listEventData.Add(eventData);
            _Sort_ListActionEvent(listEventData);

            EditorUtility.SetDirty(this);
            return eventData;
        }
        
        public void Editor_RemoveActionEvent(ActionEventDataBase eventData)
        {
            listEventData.Remove(eventData);

            switch (eventData)
            {
                /* Tag :: Define New Event :: Remove Container  */
                case ActionEventData_Animation               eventData_Animation              : _listAnimation              .Remove(eventData_Animation              ); break;  
                case ActionEventData_IgnoreGravity           eventData_IgnoreGravity          : _listIgnoreGravitie         .Remove(eventData_IgnoreGravity          ); break; 
                case ActionEventData_MovePos_ToForward       eventData_MovePos_ToForward      : _listMovePos_ToForward      .Remove(eventData_MovePos_ToForward      ); break; 
                case ActionEventData_MovePos_ToInput         eventData_MovePos_ToInput        : _listMovePos_ToInput        .Remove(eventData_MovePos_ToInput        ); break; 
                case ActionEventData_MovePos_ToBrainTarget   eventData_MovePos_ToBrainTarget  : _listMovePos_ToBrainTarget  .Remove(eventData_MovePos_ToBrainTarget  ); break; 
                case ActionEventData_MoveVel_ToForward       eventData_MoveVel_ToForward      : _listMoveVel_ToForward      .Remove(eventData_MoveVel_ToForward      ); break; 
                case ActionEventData_MoveVel_ToInput         eventData_MoveVel_ToInput        : _listMoveVel_ToInput        .Remove(eventData_MoveVel_ToInput        ); break; 
                case ActionEventData_MoveVel_ToBrainTarget   eventData_MoveVel_ToBrainTarget  : _listMoveVel_ToBrainTarget  .Remove(eventData_MoveVel_ToBrainTarget  ); break; 
                case ActionEventData_RotateAngle_Left        eventData_RotateAngle_Left       : _listRotateAngle_Left       .Remove(eventData_RotateAngle_Left       ); break; 
                case ActionEventData_RotateAngle_Right       eventData_RotateAngle_Right      : _listRotateAngle_Right      .Remove(eventData_RotateAngle_Right      ); break; 
                case ActionEventData_RotateDir_ToInput       eventData_RotateDir_ToInput      : _listRotateDir_ToInput      .Remove(eventData_RotateDir_ToInput      ); break; 
                case ActionEventData_RotateDir_ToBrainTarget eventData_RotateDir_ToBrainTarget: _listRotateDir_ToBrainTarget.Remove(eventData_RotateDir_ToBrainTarget); break; 
                case ActionEventData_RotateDir_ToMove        eventData_RotateDir_ToMove       : _listRotateDir_ToMove       .Remove(eventData_RotateDir_ToMove       ); break; 
                case ActionEventData_Hit                     eventData_Hit                    : _listHit                    .Remove(eventData_Hit                    ); break; 
                case ActionEventData_FX                      eventData_FX                     : _listFX                     .Remove(eventData_FX                     ); break; 
                case ActionEventData_SFX                     eventData_SFX                    : _listSFX                    .Remove(eventData_SFX                    ); break; 
            }

            EditorUtility.SetDirty(this);
        }

        public void Editor_ShowCreateActionEventMenu(System.Action<ActionEventDataBase> SelectContextCallback)
        {
            void _onSelectMenu(Type T) => SelectContextCallback?.Invoke(Editor_CreateActionEventData(T));
            
            var menu = new GenericMenu();

            /* Tag :: Define New Event :: AddEventMenu */
            menu.AddItem(new GUIContent("Animation"              ), false, () => _onSelectMenu(typeof(ActionEventData_Animation              )));
            menu.AddItem(new GUIContent("IgnoreGravity"          ), false, () => _onSelectMenu(typeof(ActionEventData_IgnoreGravity          )));
            menu.AddItem(new GUIContent("MovePos/ToForward"      ), false, () => _onSelectMenu(typeof(ActionEventData_MovePos_ToForward      )));
            menu.AddItem(new GUIContent("MovePos/ToInput"        ), false, () => _onSelectMenu(typeof(ActionEventData_MovePos_ToInput        )));
            menu.AddItem(new GUIContent("MovePos/ToBrainTarget"  ), false, () => _onSelectMenu(typeof(ActionEventData_MovePos_ToBrainTarget  )));
            menu.AddItem(new GUIContent("MoveVel/ToForward"      ), false, () => _onSelectMenu(typeof(ActionEventData_MoveVel_ToForward      )));
            menu.AddItem(new GUIContent("MoveVel/ToInput"        ), false, () => _onSelectMenu(typeof(ActionEventData_MoveVel_ToInput        )));
            menu.AddItem(new GUIContent("MoveVel/ToBrainTarget"  ), false, () => _onSelectMenu(typeof(ActionEventData_MoveVel_ToBrainTarget  )));
            menu.AddItem(new GUIContent("RotateAngle/Left"       ), false, () => _onSelectMenu(typeof(ActionEventData_RotateAngle_Left       )));
            menu.AddItem(new GUIContent("RotateAngle/Right"      ), false, () => _onSelectMenu(typeof(ActionEventData_RotateAngle_Right      )));
            menu.AddItem(new GUIContent("RotateDir/ToInput"      ), false, () => _onSelectMenu(typeof(ActionEventData_RotateDir_ToInput      )));
            menu.AddItem(new GUIContent("RotateDir/ToBrainTarget"), false, () => _onSelectMenu(typeof(ActionEventData_RotateDir_ToBrainTarget)));
            menu.AddItem(new GUIContent("RotateDir/ToMove"       ), false, () => _onSelectMenu(typeof(ActionEventData_RotateDir_ToMove       )));
            menu.AddItem(new GUIContent("Hit"                    ), false, () => _onSelectMenu(typeof(ActionEventData_Hit                    )));
            menu.AddItem(new GUIContent("FX"                     ), false, () => _onSelectMenu(typeof(ActionEventData_FX                     )));
            menu.AddItem(new GUIContent("SFX"                    ), false, () => _onSelectMenu(typeof(ActionEventData_SFX                    )));

            menu.ShowAsContext();
        }

        public ActionEventDataBase Editor_CreateActionEventData(System.Type eventType)
        {
            if (eventType.IsAbstract || !eventType.IsSubclassOf(typeof(ActionEventDataBase)))
                return null;

            var eventData = System.Activator.CreateInstance(eventType) as ActionEventDataBase;
            if (eventData == null)
                return null;
            
            eventData.Editor_EndFrame = this.Frame;
            return Editor_AddActionEventData(eventData);
        }
        
        public static string Editor_GetActionEventDisplayTypeName(System.Type type)
        {
            return type.Name.Replace("ActionEventData_", "");
        }
        
        private void _Sort_ListActionEvent<T>(List<T> list) where T : ActionEventDataBase
        {
            int _GetActionEvnetSortOrder(ActionEventDataBase eventData)
            {
                if (eventData == null)
                    return int.MaxValue;

                switch (eventData)
                {
                    /* Tag :: Define New Event :: SortOrder  */
                    case ActionEventData_Animation              : return 0;
                    case ActionEventData_IgnoreGravity          : return 100;
                    case ActionEventData_MovePos_ToBrainTarget  : return 1000;
                    case ActionEventData_MovePos_ToForward      : return 1000;
                    case ActionEventData_MovePos_ToInput        : return 1000;
                    case ActionEventData_MoveVel_ToBrainTarget  : return 1000;
                    case ActionEventData_MoveVel_ToForward      : return 1000;
                    case ActionEventData_MoveVel_ToInput        : return 1000;
                    case ActionEventData_RotateAngle_Left       : return 200;
                    case ActionEventData_RotateAngle_Right      : return 200;
                    case ActionEventData_RotateDir_ToBrainTarget: return 200;
                    case ActionEventData_RotateDir_ToInput      : return 200;
                    case ActionEventData_RotateDir_ToMove       : return 1100;
                    case ActionEventData_Hit                    : return 10;
                    case ActionEventData_FX                     : return 2000;
                    case ActionEventData_SFX                    : return 2001;
                }

                return int.MaxValue;
            }
            
            list.Sort((l, r) =>
            {
                var result = l.startTime.CompareTo(r.startTime);
                if (result != 0)
                    return result;

                result = _GetActionEvnetSortOrder(l).CompareTo(_GetActionEvnetSortOrder(r));
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