using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class SliderManager : MonoBehaviour
{
    public GameObject sliderPrefab; // assign in Inspector
    public Transform contentParent; // the GridLayoutGroup transform
    public Button saveButton;

    private List<Slider> sliders = new List<Slider>();
    private float[] sliderValues = new float[100];

    void Start()
    {
        Debug.Log("hello");
        GenerateSliders();
        saveButton.onClick.AddListener(SaveSliderValues);
    }

    void GenerateSliders()
    {
        for (int i = 0; i < 100; i++)
        {
            GameObject sliderGO = Instantiate(sliderPrefab, contentParent);
            Slider slider = sliderGO.GetComponentInChildren<Slider>();
            TMP_Text valueText = sliderGO.GetComponentInChildren<TMP_Text>();

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.01f; // default initial value

            valueText.text = slider.value.ToString("0.00%");

            TMP_Text capturedText = valueText;
            Slider capturedSlider = slider;

            slider.onValueChanged.AddListener((float newVal) =>
            {
                float total = sliders.Sum(s => s == capturedSlider ? 0f : s.value);
                float remaining = 1.0f - total;

                float finalValue = Mathf.Min(newVal, remaining);

                if(finalValue != newVal) {
                    capturedSlider.SetValueWithoutNotify(finalValue);
                }

                capturedText.text = finalValue.ToString("0.00");
            });


            sliders.Add(slider);

        }
    }

    void SaveSliderValues()
    {
        for (int i = 0; i < sliders.Count; i++)
        {
            sliderValues[i] = sliders[i].value;
        }

        Debug.Log("Slider values saved:");
        foreach (float val in sliderValues)
        {
            Debug.Log(val);
        }

        // Do something with sliderValues...
    }
}
