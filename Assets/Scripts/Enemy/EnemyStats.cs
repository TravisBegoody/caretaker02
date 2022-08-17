using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/DefaultEnemy")]
public class EnemyStats : ScriptableObject
{
    public float Health;

    //How fast the enemy walks
    public float WalkSpeed;
    //How enemy moves multipied for sprint
    public float SprintMultiplier;
    //How fast the tank turns with HorizontalInput
    public float TurnSpeed;
    //How far the tank can see
    public float SightRange;
    // In Degrees
    public float SightFOV;

    public float MeleeRange;
    public float MeleeDamage;
    //How far the tank can hear
    public float HearRange;
}
