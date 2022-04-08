using UnityEngine;
public interface IDamagable
{
    void TakeDamage(float _damage, int ID);

    void TakeKnockBack(float _knockF, Vector3 dir);
}