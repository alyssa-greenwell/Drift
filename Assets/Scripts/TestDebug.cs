using UnityEngine;

public class TestDebug : MonoBehaviour
{
    void Awake() { Debug.Log("TEST AWAKE WORKS!"); }
    void Start() { Debug.Log("TEST START WORKS!"); }
    void OnEnable() { Debug.Log("TEST ONENABLE WORKS!"); }
}