using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField]float rcsThrust = 100f;
    [SerializeField]float mainThrust= 100f;
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip levelLoadSound;

    [SerializeField]ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem deathSoundParticles;
    [SerializeField] ParticleSystem levelLoadSoundParticles;

    Rigidbody rigidBody;
    AudioSource audioSource;

    bool CollisionEnabled = true;

    enum State { Alive, Dead, Trancending}
    State state = State.Alive;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive) { 
            RespondToThrustInput();
            RespondToRotateInput();
        }
        //Only if debug on
        if (Debug.isDebugBuild)
        {
            RespondToDebug();
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || !CollisionEnabled)
        {
            return;
        }
        switch (collision.gameObject.tag)
        {
            case "Friendly":
                state = State.Alive;
                break;
            case "Ending":
                CommenceLevelComplete();
                break;
            case "Enemies":
                CommenceDeath();
                break;
        }
    }
    private void RespondToDebug()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(1);
        }else if (Input.GetKeyDown(KeyCode.C))
        {
            CollisionEnabled = !CollisionEnabled;
        }
    }

    private void CommenceLevelComplete()
    {
        state = State.Trancending;
        Invoke("LoadNewLevel", levelLoadDelay);
        audioSource.Stop();
        levelLoadSoundParticles.Play();
        audioSource.PlayOneShot(levelLoadSound);
    }

    private void CommenceDeath()
    {
        state = State.Dead;
        Invoke("LoadFirstLevel", levelLoadDelay);
        audioSource.Stop();
        deathSoundParticles.Play();
        audioSource.PlayOneShot(deathSound);
    }

    private void LoadNewLevel()
    {

        int currentScene =  SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentScene +1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }
        SceneManager.LoadScene(nextSceneIndex);
        state = State.Alive;
    }
    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
        state = State.Alive;
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }else
        {
            audioSource.Pause();
            mainEngineParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
            mainEngineParticles.Play();
        }
    }

    private void RespondToRotateInput()
    {
        float rotationThisFrame = rcsThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
        {
            rigidBody.freezeRotation = true;
            transform.Rotate(Vector3.forward * rotationThisFrame);
            rigidBody.freezeRotation = false;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rigidBody.freezeRotation = true;
            transform.Rotate(-Vector3.forward * rotationThisFrame);
            rigidBody.freezeRotation = false;
        }
        
    }
}
