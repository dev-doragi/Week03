using UnityEngine;

public class EnemyProjectile : ProjectileBase
{
    public void Initialize(Vector2 direction, float speed, int damage, float lifeTime, GameObject owner)
    {
        InitializeProjectile(direction, speed, lifeTime, damage, owner);
    }
}