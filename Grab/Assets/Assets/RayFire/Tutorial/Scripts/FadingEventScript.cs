using UnityEngine;

// IMPORTANT! You should add RayFire namespace to use RayFire component's event.
using RayFire;

// Tutorial script. Allows to subscribe to Rigid component fading.
public class FadingEventScript : MonoBehaviour
{
    // Define if script should subscribe to global fading event
    public bool globalSubscription = false;
    
    // Local Rigid component which will be checked for fading.
    // You can get RayfireRigid component which you want to check for fading in any way you want.
    // This is just a tutorial way to define it.
    public bool localSubscription = false;
    public RayfireRigid localRigidComponent;
    
    // /////////////////////////////////////////////////////////
    // Subscribe/Unsubscribe
    // /////////////////////////////////////////////////////////
    
    // Subscribe to event
    void OnEnable()
    {
        // Subscribe to global fading event. Every fading will invoke subscribed methods. 
        if (globalSubscription == true)
            RFFadingEvent.GlobalEvent += GlobalMethod;
        
        // Subscribe to local fading event. Fading of specific Rigid component will invoke subscribed methods. 
        if (localSubscription == true && localRigidComponent != null)
            localRigidComponent.fading.fadingEvent.LocalEvent += LocalMethod;
    }
    
    // Unsubscribe from event
    void OnDisable()
    {
        // Unsubscribe from global fading event.
        if (globalSubscription == true)
            RFFadingEvent.GlobalEvent -= GlobalMethod;
        
        // Unsubscribe from local fading event.
        if (localSubscription == true && localRigidComponent != null)
            localRigidComponent.fading.fadingEvent.LocalEvent -= LocalMethod;
    }

    // /////////////////////////////////////////////////////////
    // Subscription Methods
    // /////////////////////////////////////////////////////////
    
    // Method for local demolition subscription
    void LocalMethod(Transform tm)
    {
        Debug.Log("Local fading: " + tm.name);
    }
    
    // Method for global demolition subscription
    void GlobalMethod(Transform tm)
    {
        Debug.Log("Global fading: " + tm.name);
    }
}
