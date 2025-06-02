using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLowPartsHitbox : MonoBehaviour
{
    public BossHitbox highHitbox;

    private void Awake()
    {
        if(highHitbox == null)
        {
            Debug.LogAssertion($"{gameObject.name}에 상위 Hitbox 연결이 안되어있습니다.");
        }
        else
        {
            highHitbox.lowHitboxList.Add(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform player = null;
        if (other.CompareTag("Player"))
        {
            player = other.transform.parent;
        }

        highHitbox.AttackOneShot(player, other);
    }
}
