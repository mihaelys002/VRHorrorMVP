using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Mirror;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools;
using UnityEditor;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.LowLevel;

[TestFixture]
public class PCPlayerMovementControllerTests 
{
    private const string TEST_SCENE_PATH = "Assets/Tests/Scenes/TestScene.unity";
    private NetworkManager networkManager;
    private PCPlayerMovementController playerController;
    private Keyboard keyboard;
    private Mouse mouse;
    private bool isInitialized;

    [UnitySetUp]
    public  IEnumerator Setup()
    {

        keyboard = InputSystem.AddDevice<Keyboard>();
        mouse = InputSystem.AddDevice<Mouse>();
        yield return null;
    }



    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return null;
        if (!isInitialized)
        {
            isInitialized = true;         

     
            // Load test scene if not already loaded
#if UNITY_EDITOR
            var asyncOperation = EditorSceneManager.LoadSceneAsyncInPlayMode(TEST_SCENE_PATH, new LoadSceneParameters(LoadSceneMode.Single));
#else
            var asyncOperation = SceneManager.LoadSceneAsync(TEST_SCENE_PATH);
#endif
            while (!asyncOperation.isDone)
                yield return null;

            // Get NetworkManager from scene
            networkManager = Object.FindFirstObjectByType<NetworkManager>();
            Assert.IsNotNull(networkManager, "NetworkManager not found in scene");

            // Start host (server + client)
            networkManager.StartHost();
            yield return new WaitForSeconds(0.1f); // Wait for host to start


            playerController = NetworkClient.localPlayer?.gameObject.GetComponent<PCPlayerMovementController>();
            Assert.IsNotNull(playerController, "PCPlayerMovementController not found on player");



        }
    }

    [UnityTest]
    public IEnumerator TestPlayerMovement_Forward()
    {
        // Get initial position
        Vector3 initialPosition = playerController.transform.position;

        //var inputHandler = playerController.GetComponent<PCInputHandler>();

        //var playerInput = playerController.GetComponent<PlayerInput>();


        //Debug.Log("currentControlScheme: " +playerInput.currentControlScheme);


        // Force a rebind of the input system
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
        InputSystem.Update();
        yield return null;
        Debug.Log("Is W key pressed: " + keyboard.wKey.isPressed);
        yield return new WaitForSeconds(3f);
        Debug.Log("Is W key pressed: " + keyboard.wKey.isPressed);

        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        InputSystem.Update();

        // Check if player moved forward
        Vector3 newPosition = playerController.transform.position;
        Assert.Greater(newPosition.z, initialPosition.z, "Player should have moved forward");
    }
    [UnityTest]
    public IEnumerator TestPlayerMovement_Backward()
    {
        Vector3 initialPosition = playerController.transform.position;

        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S));
        InputSystem.Update();
        yield return new WaitForSeconds(0.5f);

        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        InputSystem.Update();

        Vector3 newPosition = playerController.transform.position;
        Assert.Less(newPosition.z, initialPosition.z, "Player should have moved backward");
    }

    //[UnityTest]
    //public IEnumerator TestPlayerJump()
    //{
    //    float initialHeight = playerController.transform.position.y;

    //    Press(keyboard.spaceKey);
    //    yield return new WaitForSeconds(0.1f);
    //    Release(keyboard.spaceKey);

    //    yield return new WaitForSeconds(0.2f);

    //    float newHeight = playerController.transform.position.y;
    //    Assert.Greater(newHeight, initialHeight, "Player should have jumped up");
    //}


    //[UnityTest]
    //public IEnumerator TestMouseLook()
    //{
    //    Quaternion initialRotation = playerController.transform.rotation;

    //    // Simulate mouse movement
    //    Set(mouse.delta, new Vector2(100, 0));
    //    yield return new WaitForSeconds(0.1f);
    //    Set(mouse.delta, Vector2.zero);

    //    Quaternion newRotation = playerController.transform.rotation;
    //    Assert.AreNotEqual(initialRotation, newRotation, "Player should have rotated");
    //}

    //[UnityTest]
    //public IEnumerator TestCoyoteTimeJump()
    //{
    //    // Move player slightly above ground
    //    playerController.transform.position += Vector3.up * 0.1f;

    //    yield return new WaitForSeconds(0.1f); // Wait to start falling

    //    // Jump just after leaving ground
    //    Press(keyboard.spaceKey);
    //    yield return new WaitForSeconds(0.1f);
    //    Release(keyboard.spaceKey);

    //    yield return new WaitForSeconds(0.2f);

    //    float newHeight = playerController.transform.position.y;
    //    Assert.Greater(newHeight, 0.1f, "Player should have performed coyote time jump");
    //}

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (networkManager != null && networkManager.isNetworkActive)
        {
            networkManager.StopHost();
        }
    }
}