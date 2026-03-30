using UnityEngine;

public class PlayerProjectile : ProjectileBase
{
    public void Initialize(Vector2 direction, float speed, float lifetime, int damage)
    {
        InitializeProjectile(direction, speed, lifetime, damage, null);
    }

    public void Initialize(Vector2 direction, float speed, float lifetime, int damage, GameObject owner)
    {
        InitializeProjectile(direction, speed, lifetime, damage, owner);
    }
}