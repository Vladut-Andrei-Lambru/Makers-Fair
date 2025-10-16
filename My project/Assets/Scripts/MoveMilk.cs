using System;
using UnityEngine;

public class MoveMilk : MonoBehaviour
{
    [SerializeField] private GameObject milkCube;
    private ParticleSystem milkParticles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        milkParticles = GetComponent<ParticleSystem>();
        milkParticles.Stop();
    }

    // Update is called once per frame
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == milkCube)
        {
            milkCube.transform.Translate(0,-0.01f,0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == milkCube)
        {
            milkParticles.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == milkCube)
        {
            milkParticles.Stop();
        }
    }
}
