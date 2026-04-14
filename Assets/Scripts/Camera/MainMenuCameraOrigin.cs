using UnityEngine;

public class MainMenuCameraOrigin : MonoBehaviour {
    public void OnDrawGizmos() {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(transform.position, new Vector3(7f * (16f/9f), 7f));
    }
}