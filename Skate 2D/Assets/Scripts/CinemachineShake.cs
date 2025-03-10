using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CinemachineShake : MonoBehaviour
{
    [SerializeField,Range(0.05f,5f)]private float defaultCameraShake = 0.5f;
    private CinemachineVirtualCamera vm;
    private CinemachineBasicMultiChannelPerlin vmPerlin;

    void Start()
    {
        vm = GetComponent<CinemachineVirtualCamera>();
        vmPerlin = vm.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float disableTimerInSeconds, float multiplier = 1)
    {
        vmPerlin.m_AmplitudeGain = defaultCameraShake * multiplier;
        StartCoroutine(DisableShake(disableTimerInSeconds));
    }

    private IEnumerator DisableShake(float time)
    {
        yield return new WaitForSeconds(time);
        vmPerlin.m_AmplitudeGain = 0;
    }
}
