using RayFire;
using UnityEngine;

public class RFParticleMan : MonoBehaviour
{
    public RFPoolingEmitter em;
    public ParticleSystem   ps;
    public Transform        tm;

    // Send particle system back to its pool
    public void OnParticleSystemStopped()
    {
        // No emitter
        if (em == null)
        {
            Destroy (gameObject);
            return;
        }

        // Should not be reused anymore
        if (em.reuse == false)
        {
            Destroy (gameObject);
            return;
        }

        // Overflow
        if (em.queue.Count >= em.over + em.cap)
        {
            Destroy (gameObject);
            return;
        }

        // Increment
        RayfireMan.inst.particles.reused++;
        
        // Remove particles
        ps.Clear();

        // Set particle system transform back to pool
        tm.SetParent (em.root);
        tm.position = em.root.position;
            
        // Add to queue
        em.queue.Enqueue (ps);
        em.SetNeed(false);
        
        // Deactivate
        tm.gameObject.SetActive (false);
    }
}
