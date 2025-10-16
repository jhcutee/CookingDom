using System;
using UnityEngine;

public class GasStoveKnob : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private GameObject flames;
    public bool isOn { get; private set;} = false;
    public event Action turnOn;
    private void OnMouseDown()
    {
        Debug.Log("AAAA");
        TurnOn();
    }
    public void TurnOn()
    {
        isOn = true;
        flames.SetActive(true);
        turnOn?.Invoke();
        this.GetComponent<Collider2D>().enabled = false; // chỉ bật 1 lần
    }
    public void TurnOff()
    {
        isOn = false;
        flames.SetActive(false);
    }
}
