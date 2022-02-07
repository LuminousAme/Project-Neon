using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FirstPersonCamExtension : CinemachineExtension
{
    private Vector3 eulerAngles;

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if(vcam.Follow)
        {
            if(stage == CinemachineCore.Stage.Aim)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    BasicPlayerController controller = playerObj.GetComponent<BasicPlayerController>();
                    if (controller != null) eulerAngles = controller.GetCamEulerAngles();
                }

                if(eulerAngles != null)
                {
                    state.RawOrientation = Quaternion.Euler(eulerAngles.y, eulerAngles.x, 0f);
                }
            }
          
        }
       


    }
}
