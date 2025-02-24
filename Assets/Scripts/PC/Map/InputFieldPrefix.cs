using TMPro;
using UnityEngine;

public class InputFieldPrefix : MonoBehaviour
{
    public TMP_InputField inputField;
    private string prefix = ">> "; // Your desired prefix

    private void Start()
    {
        if (inputField != null)
        {
            inputField.text = prefix; // Initialize with prefix
            inputField.onValueChanged.AddListener(OnTextChanged);
            inputField.onSelect.AddListener(OnSelect);
        }
    }

    private void OnTextChanged(string value)
    {
        if (!value.StartsWith(prefix))
        {
            inputField.text = prefix;
            inputField.caretPosition = inputField.text.Length; // Keep caret at the end
        }
    }

    private void OnSelect(string value)
    {
        if (!inputField.text.StartsWith(prefix))
        {
            inputField.text = prefix;
        }
        inputField.caretPosition = inputField.text.Length; // Move caret after prefix
    }
}
