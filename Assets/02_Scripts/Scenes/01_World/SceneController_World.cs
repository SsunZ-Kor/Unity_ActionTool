using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class SceneController_World : SceneControllerBase
{
    public class InitParams : SceneControllerBase.InitParams
    {
        
    }

    public override IEnumerator OnScene_Init(SceneControllerBase.InitParams initParams)
    {
        var worldInitParam = initParams as InitParams;
        if (worldInitParam != null)
        {
            
        }

        Managers.UI.OpenWindow<Window_World_Main>();
        
        var actor = Managers.Actor.GenerateActor(0, "prf_Model_10010010");
        actor.BrainCtrl.ChangeBrainType(BrainTypes.UserInput);
        actor.ActionCtrl.AddAction("Action_10010010_Idle");
        actor.ActionCtrl.AddAction("Action_10010010_Idle_Air");
        actor.ActionCtrl.AddAction("Action_10010010_Run");
        actor.ActionCtrl.AddAction("Action_10010010_Jump");
        actor.ActionCtrl.AddAction("Action_10010010_Jump_Forward");
        actor.ActionCtrl.AddAction("Action_10010010_Landing");
        actor.ActionCtrl.AddAction("Action_10010010_Slide");
        
        actor.ActionCtrl.AddAction("Action_10010010_Atk_01");
        actor.ActionCtrl.AddAction("Action_10010010_Atk_02");
        actor.ActionCtrl.AddAction("Action_10010010_Atk_03");

        yield return actor.ActionCtrl.InitActions(null);
        
        actor.ActionCtrl.PlayAction("Action_10010010_Idle");
        actor.gameObject.layer = LayerMask.NameToLayer("World_Actor_00");
        
        Managers.Cam.FollowVCam.SetFollowTarget(actor.transform);
        Managers.Cam.FollowVCam.SetOffset(1, 1.5f, -7.5f, true);

        yield return base.OnScene_Init(initParams);
    }
}