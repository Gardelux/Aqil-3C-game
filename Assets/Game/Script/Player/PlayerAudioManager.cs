using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
  //Audio Related
    [Header("Clip and Source")]
    [SerializeField] AudioSource playerSource;
    [SerializeField] AudioClip[] movementAudio;
    [SerializeField] AudioClip punchAudio;

  //Class
    [Header("Script")]
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] InputManager input;

  //Listener
    private bool playerIsRunning;
    private bool playerIsPunching;
    

    private void Start()
    {
        playerIsRunning = false;
        playerIsPunching = false;

        input.OnSprintInput += sprintListener;
        playerMovement.punchingEvent += punchListener;
    }

    private void OnDestroy()
    {
        input.OnSprintInput -= sprintListener;
        playerMovement.punchingEvent -= punchListener;
    }

    private void PlayMovement()
    {
        if (!playerIsRunning && !playerIsPunching)
        {
            if (playerSource.clip != null && playerSource.isPlaying == false)
            {
                playerSource.Play();
            }
        }
    }

    private void StopGliding()
    {
        if (playerSource.isPlaying)
        {
            playerSource.Stop();
        }
    }

    private void PlaySprint()
    {
        if(playerIsRunning)
        {
            if (playerSource.clip != null)
            {
                playerSource.Play();
            }
        }
    }

    private void PunchMovement()
    {
        if (playerIsPunching)
        {
            playerSource.clip = punchAudio;
            playerSource.Play();
        }
    }

    private void ChangeMovementAudio(int audioNumber)
    {
        if(audioNumber < 3)
        {
            playerSource.clip = movementAudio[audioNumber];

            if (audioNumber == 1)
            {
                playerSource.volume = 0.5f;
            }

            else if(audioNumber == 2)
            {
                playerSource.volume = 0.6f;
                playerSource.loop = true;
            }

            else
            {
                playerSource.volume = 1f;
                playerSource.loop = false;
            }

            
        }

        else if(audioNumber == 4) //crouch Audio
        {
            playerSource.Stop();
            playerSource.clip = movementAudio[0];
            playerSource.volume = 0.4f;
        }
    }


  //Listener Method Section
    private void sprintListener(bool isRunning)
    {
        if (isRunning)
        {
            playerIsRunning = true;
        }

        else
        {
            playerIsRunning = false;
        }
    }

    private void punchListener(bool isPunching)
    {
        if (isPunching)
        {
            playerIsPunching = true;
        }

        else
        {
            playerIsPunching = false;
        }
    }
}
