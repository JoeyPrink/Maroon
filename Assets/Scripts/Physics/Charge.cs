﻿using System.Collections;
using UnityEngine;

public class Charge : MonoBehaviour
{
    [SerializeField]
    private ulong id;

    [SerializeField]
    private float chargeValue;

    [SerializeField]
    private float mass;

    [SerializeField]
    private bool justCreated = true;

    private CapacitorPlateController plate;

    private ChargePoolHandler chargePoolHandler;

    public ulong Id
    {
        get { return id; }
        set { id = value; }
    }

    public CapacitorPlateController Plate
    {
        get { return plate; }
        set { plate = value; }
    }

    public ChargePoolHandler ChargePoolHandler
    {
        get { return chargePoolHandler; }
        set { chargePoolHandler = value; }
    }

    public float ChargeValue
    {
        get { return chargeValue; }
    }

    public float Mass
    {
        get { return mass; }
    }

    public bool JustCreated
    {
        get { return justCreated; }
        set { justCreated = value; }
    }

    public void FadingOut(float fadingOutTime)
    {
        StartCoroutine("FadeOutSequence", fadingOutTime);
    }

    private IEnumerator FadeOutSequence(float fadingOutTime)
    {
        float fadingOutSpeed = 1.0f / fadingOutTime;

        Renderer rendererObj = GetComponent<Renderer>();
        float alphaValue = rendererObj.material.color.a;

        while (alphaValue >= 0)
        {
            alphaValue -= Time.deltaTime * fadingOutSpeed;
            Color newColor = rendererObj.material.color;
            newColor.a = alphaValue;
            rendererObj.material.SetColor("_Color", newColor);
            yield return null;
        }

        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("ElectricField"))
            return;

        Vector3 force = Vector3.zero;
        GetComponent<Rigidbody>().AddForce(force);
    }

    private void OnDestroy()
    {
        if (plate != null)
            plate.RemoveCharge(this);

        if (chargePoolHandler != null)
            chargePoolHandler.RemoveCharge(this);
    }
}
