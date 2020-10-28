using System;
using UnityEngine;

[Serializable]
public class PlayerStats : ScriptableObject
{
    public float speed = 7.5f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;
    public float maxBurrowSpeed = 40f;
    public float startBurrowSpeed = 1f;
    public float burrowAcceleration;
    public float burrowDeceleration;
    public float burrowTurnRate;
    public float unburrowJump = 2f;
    public float maxTimingBoost = 5f;
    public float boostRange = 5f;
    public float burrowHeight = 10f;
    public float slopeBoost = 10f;
}