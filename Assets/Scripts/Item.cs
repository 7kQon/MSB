﻿using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using MoreMountains.Tools;
using MSBNetwork;

public class Item : MonoBehaviour
{
    protected int Room;
    protected MSB_Character Target;
    protected int TargetNum;

    public MMFeedbacks ItemSpawnFeedback;
    public MMFeedbacks ItemTakenFeedback;
    
    // Start is called before the first frame update
    protected virtual  void Start()
    {
        Room = GameInfo.Instance.room;
        ItemSpawnFeedback?.Initialization();
        ItemSpawnFeedback?.Initialization();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Target = other.gameObject.MMGetComponentNoAlloc<MSB_Character>();
        if (Target != null)
            TargetNum = Target.UserNum;
        else
        {
            Debug.LogWarning("Trigger is not Msb Character");
        }
    }

    protected virtual void OnEnable()
    {
        ItemSpawnFeedback?.PlayFeedbacks();
    }

    protected void OnDisable()
    {
        ItemTakenFeedback?.PlayFeedbacks();
    }
}
