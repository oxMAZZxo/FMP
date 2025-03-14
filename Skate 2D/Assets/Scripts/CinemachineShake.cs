using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

/// <summary>
/// This is a custom component that can be attached on a cinemachine virtual camera to add camera shake effects.
/// Note that it requires that the GameObject it is attached to, already has a cinemachine virtual camera component, and Perlin Noise added to that component. 
/// </summary>
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineShake : MonoBehaviour
{
    [SerializeField,Range(0.05f,5f)]private float defaultCameraShake = 0.5f;
    private CinemachineVirtualCamera vm;
    private CinemachineBasicMultiChannelPerlin vmPerlin;

    void Start()
    {
        vm = GetComponent<CinemachineVirtualCamera>();
        vmPerlin = vm.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        vmPerlin.m_AmplitudeGain = 0;
    }

    /// <summary>
    /// Will enable the camera shake for the specified time, with an optional multiplier.
    /// </summary>
    /// <param name="disableTimerInSeconds">The amount of time you want the shake to last</param>
    /// <param name="multiplier">The multiplier to add to the default camera shake value. This value needs to be bigger than 1!</param>
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
