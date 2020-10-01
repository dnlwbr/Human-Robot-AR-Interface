using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HumanRobotInterface
{
    public class TheTime : MonoBehaviour
    {
        DateTime localDate;
        private TextMeshPro TextMeshProComponent;
        private CultureInfo culture;

        // Start is called before the first frame update
        void Start()
        {
            TextMeshProComponent = GetComponent<TextMeshPro>();
            culture = CultureInfo.CreateSpecificCulture("de-DE");
        }

        // Update is called once per frame
        void Update()
        {
            localDate = DateTime.Now;
            TextMeshProComponent.text = localDate.ToString("t", culture);
        }
    }
}