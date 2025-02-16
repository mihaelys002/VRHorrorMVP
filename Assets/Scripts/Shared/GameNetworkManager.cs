using Mirror;
using UnityEngine;

public class GameNetworkManager : NetworkManager
{
    [SerializeField] private GameObject pcPlayerPrefab;
    [SerializeField] private GameObject vrPlayerPrefab;

    // You'll need to implement a way to determine if the connecting player is VR or PC
    private bool IsVRPlayer(NetworkConnectionToClient conn)
    {
        // Implement your logic here
        return false;
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject prefabToSpawn = IsVRPlayer(conn) ? vrPlayerPrefab : pcPlayerPrefab;
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(prefabToSpawn, startPos.position, startPos.rotation)
            : Instantiate(prefabToSpawn);

        NetworkServer.AddPlayerForConnection(conn, player);
    }
}