﻿using Maroon.Physics;
using GEAR.Localization;
using TMPro;
using UnityEngine;

public class VoltmeterDifferences : MonoBehaviour
{
    public VoltmeterMeasuringPoint positiveMeasuringPoint;
    public VoltmeterMeasuringPoint negativeMeasuringPoint;

    private CoulombLogic _coulombLogic;
    public TextMeshPro textMeshPro;
    public TextMeshProUGUI textMeshProGUI;
    public TextMeshPro textMeshProUnit;

    public bool onPerDefault = true;
    public bool showUnitInText = true;

    [Header("Assessment System")]
    public QuantityFloat currentValue = 0;
    public QuantityBool voltmeterEnabled = true;

    private bool _isOn;

    private Vector3 positiveMeasuringPosition => GetMeasuringPosition(positiveMeasuringPoint);
    private Vector3 negativeMeasuringPosition => GetMeasuringPosition(negativeMeasuringPoint);

    private Vector3 GetMeasuringPosition(VoltmeterMeasuringPoint measuringPoint)
    {
        if (_coulombLogic.IsIn2dMode())
        {
            var pos = measuringPoint.transform.position;
            var position = _coulombLogic.xOrigin2d.position;
            return new Vector3(_coulombLogic.WorldToCalcSpace(pos.x - position.x),
                _coulombLogic.WorldToCalcSpace(pos.y - position.y),
                0);
        }
        else
        {
            var pos = measuringPoint.transform.localPosition;
            var position = _coulombLogic.xOrigin3d.localPosition;
            return new Vector3(_coulombLogic.WorldToCalcSpace(pos.x - position.x, true),
                _coulombLogic.WorldToCalcSpace(pos.y - position.y, true),
                _coulombLogic.WorldToCalcSpace(pos.z - position.z, true));
        }
    }

    private float MeasuringPointDistance => _coulombLogic.WorldToCalcSpace(Vector3.Distance(positiveMeasuringPoint.transform.position, negativeMeasuringPoint.transform.position));


    private void Start()
    {
        if(textMeshPro == null)
            textMeshPro = GetComponent<TextMeshPro>();

        if (textMeshProGUI == null)
            textMeshProGUI = GetComponent<TextMeshProUGUI>();
        
        var simControllerObject = GameObject.Find("CoulombLogic");
        if (simControllerObject)
            _coulombLogic = simControllerObject.GetComponent<CoulombLogic>();
        Debug.Assert(_coulombLogic != null);

        _isOn = onPerDefault;
    }

    private void Update()
    {
        if (!_isOn || (!positiveMeasuringPoint.isActiveAndEnabled || !negativeMeasuringPoint.isActiveAndEnabled))
            SetText("--- " + (showUnitInText? GetCurrentUnit() : ""));
        else
        {
            var displayText = $"{LanguageManager.Instance.GetString("Voltage")}: {GetDifference()} {(showUnitInText ? " " + GetCurrentUnit() : "")}\n\n" +
                              $"{LanguageManager.Instance.GetString("VoltmeterPositiveKey")}: {positiveMeasuringPosition}\n" +
                              $"{LanguageManager.Instance.GetString("VoltmeterNegativeKey")}: {negativeMeasuringPosition}\n" +
                              $"{LanguageManager.Instance.GetString("Distance")}: {MeasuringPointDistance:0.##} m";

            SetText(displayText);
        }

        if (!showUnitInText && textMeshProUnit)
        {
            textMeshProUnit.text = GetCurrentUnit();
        }
    }

    private void SetText(string text)
    {
        if (textMeshPro)
            textMeshPro.text = text;
        if (textMeshProGUI)
            textMeshProGUI.text = text;
    }
    
    private string GetDifference()
    {
        var currentDifference = positiveMeasuringPoint.GetPotentialInMicroVolt() -
                                negativeMeasuringPoint.GetPotentialInMicroVolt();
        
        if(Mathf.Abs(currentDifference - currentValue.Value) > 0.000001)
            currentValue.Value = currentDifference;
        return GetCurrentFormattedString();
    }

    private string GetCurrentFormattedString()
    {
        float check = currentValue.Value;
        for (var cnt = 0; Mathf.Abs(check) < 1f && cnt < 2; ++cnt)
        {
            check *= Mathf.Pow(10, 3);
        }
            
//        Debug.Log("START: " + _currentValue.ToString("F") + " - END: "+ check.ToString("F"));
        return check.ToString("F");   
    }

    private string GetCurrentUnit()
    {
        var unit = "V";
        var check = Mathf.Abs(currentValue.Value);
        if (check > 1f)
            return unit;
        check *= Mathf.Pow(10, 3);
        if (check > 1f)
            return "m" + unit;
        return "\u00B5" + unit;
    }

    public void TurnOn()
    {
        _isOn = true;
    }

    public void TurnOff()
    {
        _isOn = false;
    }

    public IQuantity GetVoltage()
    {
        return currentValue;
    }

    public void Enable(bool enable)
    {
        if (!enable)
        {
            positiveMeasuringPoint.HideObject();
            negativeMeasuringPoint.HideObject();
        }
        else {
            //do nothing the user just has to pull in the voltmeter measuring points and now he can
        }
    }

    public void InvokeValueChangedEvent()
    {
        if (!positiveMeasuringPoint.isActiveAndEnabled && !negativeMeasuringPoint.isActiveAndEnabled)
            currentValue.Value = 0f;
        else
            GetDifference(); //call to update the current value, seems as otherwise this will be done too late
        currentValue.SendValueChangedEvent();
    }

    public void AddValueChangedEventsToCharge(CoulombChargeBehaviour charge)
    {
        if (!charge) return;

        charge.charge.onValueChanged.AddListener(newVal => InvokeValueChangedEvent());
        
        var dragHandler = charge.GetComponent<CoulombAssessmentPosition>();
        if (dragHandler)
        {
            dragHandler.onUpdateMessageSend.AddListener(InvokeValueChangedEvent);
        }

    }
}
