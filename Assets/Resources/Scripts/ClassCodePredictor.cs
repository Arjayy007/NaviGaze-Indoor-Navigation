using UnityEngine;
using UnityEngine.UI;


public class ClassCodePredictor : MonoBehaviour
{
    public InputField inputClassCode;
    public InputField outputSubjectName; // or TMP_Text if you don't want it editable

    void Start()
    {
        // Add listener to run when text changes
        inputClassCode.onValueChanged.AddListener(OnInputChanged);
    }

    void OnInputChanged(string input)
    {
        string result = ClassCodeDictionary.GetSubjectName(input.Trim());
        outputSubjectName.text = result;
    }
}
