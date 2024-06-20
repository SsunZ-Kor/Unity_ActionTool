using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public partial class ActionController
    {
        private SortedDictionary<string, ActionRuntime> _dicActionRuntimes = new();

        public IEnumerator InitActions(System.Action endCallback)
        {
            foreach (var actionRuntime in _dicActionRuntimes.Values)
                yield return actionRuntime.Init(Master, null);
            
            endCallback?.Invoke();
        }
        
        public void AddAction(ActionData actionData)
        {
            if (_dicActionRuntimes.ContainsKey(actionData.name))
            {
#if UNITY_EDITOR
                Debug.LogError($"Add ActionData Failed // {Master.name} // {actionData.name}");
#endif
                return;
            }

            _dicActionRuntimes.Add(actionData.name, new(Master, actionData));
        }

        public void AddAction(string actionName)
        {
            AddAction(Managers.Actor.GetActionData(actionName));
        }

        public void RemoveActionData(ActionData actionData)
        {
            if (actionData != null)
                RemoveActionData(actionData.name);
        }

        public void RemoveActionData(string actionName)
        {
            var actionRuntime = _dicActionRuntimes.GetOrNull(actionName);
            if (actionRuntime == null)
                return;

            actionRuntime.OnFinalize(Master);
            _dicActionRuntimes.Remove(actionName);
        }

        public void ClearActionData(IEnumerable<ActionData> ignoreActionData)
        {
            if (ignoreActionData == null)
            {
                foreach (var pair in _dicActionRuntimes)
                    pair.Value.OnFinalize(Master);

                _dicActionRuntimes.Clear();
            }
            else
            {
                HashSet<ActionData> setIgnore = new(ignoreActionData);
                List<ActionRuntime> listIgnore = new(setIgnore.Count);

                foreach (var pair in _dicActionRuntimes)
                {
                    var actionRuntime = pair.Value;
                    if (setIgnore.Contains(actionRuntime.Data))
                    {
                        listIgnore.Add(actionRuntime);
                        continue;
                    }

                    actionRuntime.OnFinalize(Master);
                }

                _dicActionRuntimes.Clear();
                foreach (var actionRuntime in listIgnore)
                    _dicActionRuntimes.Add(actionRuntime.Data.name, actionRuntime);
            }
            
            _crrActionEventRuntimesForUpdate.Clear();
        }

        public void ClearActionData(IEnumerable<string> ignoreActionName = null)
        {
            if (ignoreActionName == null)
            {
                foreach (var pair in _dicActionRuntimes)
                    pair.Value.OnFinalize(Master);

                _dicActionRuntimes.Clear();
            }
            else
            {
                HashSet<string> setIgnore = new(ignoreActionName);
                List<ActionRuntime> listIgnore = new(setIgnore.Count);

                foreach (var pair in _dicActionRuntimes)
                {
                    var actionRuntime = pair.Value;
                    if (setIgnore.Contains(actionRuntime.Data.name))
                    {
                        listIgnore.Add(actionRuntime);
                        continue;
                    }

                    actionRuntime.OnFinalize(Master);
                }

                _dicActionRuntimes.Clear();
                foreach (var actionRuntime in listIgnore)
                    _dicActionRuntimes.Add(actionRuntime.Data.name, actionRuntime);
            }
            
            _crrActionEventRuntimesForUpdate.Clear();
        }
    }
}