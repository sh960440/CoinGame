using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private Animator coinAnimator;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void POP()
    {
        PlayerInfo.instance.IncreasePops();
        PlayFabController.instance.SetStats();
        coinAnimator.SetTrigger("PlayAnimation");
        audioSource.Play();
    }
}
