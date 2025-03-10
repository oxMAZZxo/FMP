using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CinemachineShake : MonoBehaviour
{
    CinemachineVirtualCamera vm;
    CinemachineBasicMultiChannelPerlin vmPerlin;

    void Start()
    {
        vm = GetComponent<CinemachineVirtualCamera>();
        vmPerlin = vm.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float disableTimerInSeconds, float multiplier = 1)
    {
        vmPerlin.m_AmplitudeGain = 0.5f * multiplier;
        StartCoroutine(DisableShake(disableTimerInSeconds));
    }

    private IEnumerator DisableShake(float time)
    {
        yield return new WaitForSeconds(time);
        vmPerlin.m_AmplitudeGain = 0;
    }
}
