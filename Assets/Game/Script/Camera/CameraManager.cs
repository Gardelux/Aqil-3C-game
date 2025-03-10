using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CameraManager : MonoBehaviour
{
  //Class
    [SerializeField] public CameraState cameraState;
    [SerializeField] InputManager _input;

   //Variable Field
    [SerializeField] CinemachineVirtualCamera _firstPersonCam;
    [SerializeField] CinemachineFreeLook _thirdPersonCam;

   //Event
    public Action OnChangePerspective;

    private void Start()
    {
        _input.OnChangePOV += SwitchCamera;
    }

    private void OnDestroy()
    {
        _input.OnChangePOV -= SwitchCamera;
    }

    public void ThirdPersonFOV(float fov)
    {
        _thirdPersonCam.m_Lens.FieldOfView = fov;
    }

    public void SetFirstPersonClamped(bool isClamped, Vector3 playerRotation)
    {
        CinemachinePOV pov = _firstPersonCam.GetCinemachineComponent<CinemachinePOV>();
        if (isClamped)
        {
            pov.m_HorizontalAxis.m_Wrap = false;
            pov.m_HorizontalAxis.m_MinValue = playerRotation.y - 65;
            pov.m_HorizontalAxis.m_MaxValue = playerRotation.y + 65;
        }

        else
        {
            pov.m_HorizontalAxis.m_Wrap = true;
            pov.m_HorizontalAxis.m_MinValue = -180;
            pov.m_HorizontalAxis.m_MaxValue = 180;
        }
    }

    private void SwitchCamera()
    {
        OnChangePerspective();
        if (cameraState == CameraState.ThirdPerson)
        {
            cameraState = CameraState.FirstPerson;
            _thirdPersonCam.gameObject.SetActive(false);
            _firstPersonCam.gameObject.SetActive(true);
        }

        else
        {
            cameraState = CameraState.ThirdPerson;
            _thirdPersonCam.gameObject.SetActive(true);
            _firstPersonCam.gameObject.SetActive(false);
        }
    }
}
