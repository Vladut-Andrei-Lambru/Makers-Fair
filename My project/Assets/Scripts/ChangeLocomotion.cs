using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Canvas = UnityEngine.Canvas;

public class ChangeLocomotion : MonoBehaviour
{
    [SerializeField] XRInputButtonReader m_MenuInput = new XRInputButtonReader("Menu");
    
    public XRInputButtonReader jumpInput
    {
        get => m_MenuInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_MenuInput, value, this);
    }

    [Header("Scene Settings")]
    [SerializeField] private int BasicSceneIndex = 0;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource cameraAudioSource; // Assign your camera's AudioSource here
    
    private ControllerInputActionManager LeftController, RightController;
    private GameObject handMenu, restartButton, quitButton, muteButton, parkButton;
    private bool isMuted = false;
    
    private void Start()
    {
        LeftController = GameObject.Find("Left Controller").GetComponent<ControllerInputActionManager>();
        RightController = GameObject.Find("Right Controller").GetComponent<ControllerInputActionManager>();
        
        quitButton = GameObject.Find("Quit Button");
        restartButton = GameObject.Find("Reset Button");
        muteButton = GameObject.Find("Mute Button");
        parkButton = GameObject.Find("Park Button");
        
        quitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Quit";
        restartButton.GetComponentInChildren<TextMeshProUGUI>().text = "Restart";
        muteButton.GetComponentInChildren<TextMeshProUGUI>().text = "Mute Music";
        parkButton.GetComponentInChildren<TextMeshProUGUI>().text = "Return to Park";
        
        handMenu = gameObject.GetComponentInChildren<Canvas>().gameObject;
        handMenu.SetActive(false);
        
        if (cameraAudioSource == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraAudioSource = mainCam.GetComponent<AudioSource>();
            }
        }
    }

    private void Update()
    {
        if (m_MenuInput.ReadIsPerformed())
        {
            handMenu.SetActive(!handMenu.activeSelf);
        }
    }

    public void Mute()
    {
        if (cameraAudioSource == null)
        {
            return;
        }
        
        isMuted = !isMuted;
        cameraAudioSource.mute = isMuted;
        
        if (muteButton != null)
        {
            var text = muteButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = isMuted ? "Unmute Music" : "Mute Music";
            }
        }
    }
    
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Park()
    {
        SceneManager.LoadScene(BasicSceneIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}