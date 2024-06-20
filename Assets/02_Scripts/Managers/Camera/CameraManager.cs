using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour, IManager
{
    public Camera MainCam { get; private set; }
    public CinemachineBrain MainCamBrain { get; private set; }

    public FollowVCamera FollowVCam { get; private set; }

    public IEnumerator Init()
    {
        /* Init MainCamera */
        var prfCamera = Resources.Load<GameObject>("Camera/MainCamera");
        var goCamera = Instantiate(prfCamera, this.transform);
        goCamera.transform.Reset();

        MainCam = goCamera.GetComponent<Camera>();
        MainCamBrain = goCamera.GetComponent<CinemachineBrain>();
        
        /* Init FollowVCamera */
        var prfFollowVCamera = Resources.Load<GameObject>("Camera/FollowVCamera");
        var goFollowVCamera = Instantiate(prfFollowVCamera, this.transform);
        goFollowVCamera.transform.Reset();

        FollowVCam = goFollowVCamera.GetComponent<FollowVCamera>();
        yield break;
    }

    public void Release()
    {
    }

    public void SetActiveFollowCam()
    {
    }
}