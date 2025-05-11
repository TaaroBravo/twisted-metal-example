using System;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public string displayName;
    public int amount;
    public Sprite icon;
    public Bullet prefab;

    private int _initialAmount;
    
    private void Start()
    {
        _initialAmount = amount;
    }

    public void Use(Transform player)
    {
        amount--;
        var bullet = Instantiate(prefab);
        bullet.transform.position = player.position + Vector3.up;
        bullet.transform.forward = player.forward;
    }

    public void IncreaseAmount()
    {
        amount += _initialAmount;
    }
}