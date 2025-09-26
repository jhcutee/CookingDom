using System;
using UnityEngine;
using UnityEngine.UI;

public class Knob : MonoBehaviour
{
    [Header("Elements")]

    [SerializeField] private GameObject RunningWater;
    public event Action turnOn;
    public bool isOn { get; private set; } = false;
    private void OnMouseDown()
    {
        Toggle();
    }
    public void Toggle() => SetFaucet(!isOn);
    private void SetFaucet(bool On)
    {
        if (isOn == On) return;
        isOn = On;
        RunningWater.SetActive(isOn);
        if (On)
        {
            turnOn?.Invoke();
        }
    }
}
