using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Data/Settings")]
public class Settings : ScriptableObject
{
    [SerializeField] private float defaultSensitivity = 0.7f;

    private float _currentSensitivity;

    private const string SENSITIVITY_PREFS_KEY = "MouseSensitivity";

    private void OnEnable()
    {
        LoadFromPlayerPrefs();
    }

    public float Sensitivity
    {
        get => _currentSensitivity;
        set
        {
            _currentSensitivity = value;
            PlayerPrefs.SetFloat(SENSITIVITY_PREFS_KEY, value);
            PlayerPrefs.Save();
        }
    }

    public void LoadFromPlayerPrefs()
    {
        _currentSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_PREFS_KEY, defaultSensitivity);
    }
    public void ResetToDefault()
    {
        Sensitivity = defaultSensitivity;
    }
}