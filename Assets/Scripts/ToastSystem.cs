using System;
using TMPro;
using UnityEngine;

public class ToastSystem : MonoBehaviour
{
    public static ToastSystem Instance;
    [SerializeField]
    private GameObject toastPanel;
    [SerializeField]
    private TextMeshProUGUI toastText;


    private void Awake() => Instance = this;

    public void Toast(string text,float timer = 3)
    {
        toastPanel.SetActive(true);
        toastText.text = text;
        AudioManager.Instance.PlayMoveClip();
        Invoke(nameof(DeactivateToastPanel), timer);
    }


    private void DeactivateToastPanel()
    {
        toastPanel.SetActive(false);
    }
}