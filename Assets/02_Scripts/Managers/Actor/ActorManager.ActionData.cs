using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public partial class ActorManager
{
    private Dictionary<string, ActionData> _dicActionData = new();

    public ActionData GetActionData(string actionName)
    {
        if (_dicActionData.TryGetValue(actionName, out var result))
            return result;
        
        result = Managers.Asset.Load<ActionData>($"Actor/{actionName}.asset");
        if (result == null)
        {
            Debug.LogError($"ActorManager.GetActionData :: Not Found ActionData \"{actionName}\"");
            return null;
        }
        result.OnAfterDeserialize();
        _dicActionData.Add(actionName, result);

        return result;
    }
}
