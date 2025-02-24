using UnityEngine;

public class AutoPlaySound: MonoBehaviour
{

    private void Start()
    {
        GetComponent<AudioSource>().Play();
    }


}
