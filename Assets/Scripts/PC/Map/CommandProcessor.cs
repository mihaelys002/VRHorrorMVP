using System.Collections;
using TMPro;
using UnityEngine;

public class CommandProcessor : MonoBehaviour
{
    [SerializeField] private TMP_InputField commandInput;
    [SerializeField] private TextMeshProUGUI commandHistory;
    [SerializeField] private CanvasGroup uiBlocker;
    [SerializeField] private MonitorManager monitorManager;

    private bool isProcessingMapCommand = false;
    private int currentAccessLevel = 0;

    public void Initialize(int accessLevel)
    {
        currentAccessLevel = accessLevel;
        commandInput.onSubmit.AddListener(ProcessCommand);
        uiBlocker.blocksRaycasts = false;
        uiBlocker.alpha = 0;
    }

    public void ExecuteCommandFromMapObject(string command)
    {
        isProcessingMapCommand = true;
        BlockUI(true);

        // Show command being typed
        StartCoroutine(SimulateTyping(command, () =>
        {
            ProcessCommand(command);
            BlockUI(false);
            isProcessingMapCommand = false;
        }));
    }

    private void BlockUI(bool block)
    {
        uiBlocker.blocksRaycasts = block;
        uiBlocker.alpha = block ? 0.1f : 0;
    }

    private IEnumerator SimulateTyping(string command, System.Action onComplete)
    {
        commandInput.text = "";
        foreach (char c in command)
        {
            commandInput.text += c;
            yield return new WaitForSeconds(0.03f);
        }
        yield return new WaitForSeconds(0.1f);
        onComplete?.Invoke();
    }

    private void ProcessCommand(string input)
    {
        AddToCommandHistory(input);

        string[] parts = input.Split(' ');
        if (parts.Length < 2)
        {
            DisplayError("Invalid command format. Use: <command> <objectId> [args]");
            return;
        }

        string command = parts[0].ToLower();
        string targetId = parts[1].ToLower();

        // Try to find the target object
        InteractiveMapObject targetObject = monitorManager.GetMapObjectById(targetId);

        if (targetObject == null)
        {
            DisplayError($"Target not found: {targetId}");
            return;
        }

        // Check access level
        if (currentAccessLevel < targetObject.RequiredAccessLevel)
        {
            DisplayError($"Access denied. Required access level: {targetObject.RequiredAccessLevel}");
            return;
        }

        // Try to execute the command
        bool success = targetObject.TryExecuteCommand(command, parts, currentAccessLevel);

        if (!success)
        {
            DisplayError($"Command '{command}' failed or is not supported for {targetId}");
        }
    }

    private void AddToCommandHistory(string command)
    {
        commandHistory.text = $"> {command}\n{commandHistory.text}";
    }

    private void DisplayError(string message)
    {
        commandHistory.text = $"<color=red>ERROR: {message}</color>\n{commandHistory.text}";
    }
}