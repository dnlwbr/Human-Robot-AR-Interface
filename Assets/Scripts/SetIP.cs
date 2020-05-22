using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetIP : MonoBehaviour
{
    public TouchScreenKeyboard keyboard;
    public static string keyboardText = "";
    private GameObject RosSharp;

    // Start is called before the first frame update
    void Start()
    {
        RosSharp = GameObject.Find("RosSharp");
    }

    public void OpenSystemKeyboard()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false);
    }

    // Update is called once per frame
    void Update()
    {
        //if (TouchScreenKeyboard.visible == false && keyboard != null)
        if (keyboard != null)
        {
            keyboardText = keyboard.text;

            if (keyboard.done == true)
            {
                RosSharp.GetComponent<RosConnector>().RosSocket.Close();
                RosSharp.GetComponent<RosConnector>().RosBridgeServerUrl = keyboardText;
                RosSharp.GetComponent<RosConnector>().Awake();
                keyboard = null;
            }
        }
    }
}
