using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public partial class ActorManager : MonoBehaviour, IManager
{
    
    public IEnumerator Init()
    {
        _OnInit_TargetingObject();
        yield break;
    }

    public void Release()
    {
        _OnRelease_TargetingObject();
        Destroy(gameObject);
    }
    
    public void OnScene_Closed()
    {
        _OnRelease_TargetingObject();
    }

    public Actor.Actor GenerateActor(int actorGroupUniqueKey, string addr_Model)
    {
        var prfModel = Managers.Asset.Load<GameObject>($"Actor/{addr_Model}.prefab");
        if (prfModel == null)
        {
            Debug.LogError($"Not Found Actor Model Prefab :: {addr_Model}");
            return null;
        }
        
        var goModel = Object.Instantiate(prfModel);
        var model = goModel.GetComponent(typeof(ModelController)) as ModelController;
        if (model == null)
        {
            Debug.LogError($"Not Found ModelController Component:: {addr_Model}");
            return null;
        }

        if (model.Anim != null)
        {
            model.Anim.applyRootMotion = false;
        }

        var prfActor = Resources.Load<GameObject>("Actor_Prefabs/Actor");
        if (prfActor == null)
        {
            Debug.LogError($"Not Found Actor Prefab :: Actor_Prefabs/Actor.prefab");
            return null;
        }
        
        var goActor = Object.Instantiate(prfActor);
        var actor = goActor.GetComponent(typeof(Actor.Actor)) as Actor.Actor;
        if (actor == null)
        {
            Debug.LogError($"Not Found Actor Component :: Actor_Prefabs/Actor.prefab");
            return null;
        }

        actor.AttachModel(model);
        return actor;
    }
}