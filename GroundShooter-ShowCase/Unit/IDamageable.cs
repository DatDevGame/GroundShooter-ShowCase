using UnityEngine;

public interface IDamageable
{
    void TakeDamage(IAttackable damageSource);
    float GetCurrentHealth();
    float GetMaxHealth();
    bool IsDead();
    Transform GetTransform();
}