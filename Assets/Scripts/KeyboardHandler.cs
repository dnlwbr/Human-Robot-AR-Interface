using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;


namespace HumanRobotInterface
{
    [RequireComponent(typeof(MixedRealityKeyboard))]
    public class KeyboardHandler : MonoBehaviour
    {
        [SerializeField]
        private MixedRealityKeyboard keyboard;
        [SerializeField]
        private MixedRealityKeyboardPreview keyboardPreview;

        public void OpenKeyboard()
        {
            keyboard.ShowKeyboard("", false);
            if (keyboardPreview != null)
            {
                keyboardPreview.gameObject.SetActive(true);
            }
        }

        public void HideKeyboard()
        {
            keyboard.HideKeyboard();
            if (keyboardPreview != null)
            {
                keyboardPreview.gameObject.SetActive(false);
                keyboardPreview.Text = string.Empty;
                keyboardPreview.CaretIndex = 0;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (keyboardPreview != null && keyboard.Visible)
            {
                keyboardPreview.Text = keyboard.Text;
                keyboardPreview.CaretIndex = keyboard.CaretIndex;
            }
        }
    }
}
