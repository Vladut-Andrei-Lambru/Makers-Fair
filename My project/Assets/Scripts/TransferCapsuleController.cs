using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransferCapsuleController : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private GameObject door;
    [SerializeField] private string sceneName;

    [Header("Timing")]
    [SerializeField] private float delayBeforeLoad = 0.8f;

    private bool _playerInside;
    private bool _transferring;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) _playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _playerInside = false;
    }

    // Hook this to the button's "Select Entered" (or call from another script)
    public void BeginTransfer()
    {
        if (_transferring || !_playerInside) return;
        _transferring = true;
        if (door) door.SetActive(true); // "close" instantly
        StartCoroutine(LoadNext());
    }

    private IEnumerator LoadNext()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}