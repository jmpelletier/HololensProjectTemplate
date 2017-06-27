using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(InputField))]
public class DictationInput : MonoBehaviour {

    InputField inputField;

    // Use this for initialization
    void Start () {
        inputField = GetComponent<InputField>();
	}

    void UpdateText (string text)
    {
        inputField.text = text;
    }

    public void GazeOn()
    {
        SpeechManager.instances.main.OnDictationUpdate += UpdateText;
        SpeechManager.instances.main.OnDictationComplete += UpdateText;

        SpeechManager.instances.main.StartDictation();
    }

    public void GazeOff()
    {
        SpeechManager.instances.main.EndDictation();

        SpeechManager.instances.main.OnDictationUpdate -= UpdateText;
        SpeechManager.instances.main.OnDictationComplete -= UpdateText;
    }
}
