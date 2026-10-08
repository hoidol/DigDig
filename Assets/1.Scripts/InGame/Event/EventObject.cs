using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EventObject : MonoBehaviour
{
    public EventType eventType;
    public Transform Transform => transform;
    public float MaxTime => maxTime;
    [SerializeField] protected float maxTime;

    public float CurTimer => curTimer;
    public Image barImage;


    [SerializeField] protected float curTimer;




    public bool interacting;
    public virtual void Appear(Vector2 spawnPos)
    {
        curTimer = maxTime;

        interacting = false;
        
    }

    public virtual void Update()
    {
        if (interacting)
            return;
        if(curTimer <= 0)
        {
            Destroy();
        }

        if (curTimer > 0)
            curTimer -= Time.deltaTime;

        barImage.fillAmount = curTimer / maxTime;
    }

    public abstract void StartEvent();



    public virtual void Destroy()
    {
        EventManager.Instance?.RemoveEventObject(this);
        Destroy(gameObject);

        ReleaseTile();
    }

    public void ReleaseTile()
    {

    }


    public bool CanHit()
    {
        return true;
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {

    }
}
