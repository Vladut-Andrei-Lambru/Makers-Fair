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

    private bool playerInside = false;
    private bool transferring = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInside = false;
    }

    // Hook this to the button's "Select Entered" (or call from another script)
    public void BeginTransfer()
    {
        if (transferring || !playerInside) return;
        transferring = true;
        if (door) door.SetActive(true); // "close" instantly
        StartCoroutine(LoadNext());
    }

    private IEnumerator LoadNext()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}