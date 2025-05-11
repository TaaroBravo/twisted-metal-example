using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    public UIManager UIManager;
    
    private List<Weapon> _currentWeapons = new List<Weapon>();
    private int _currentWeaponIndex;

    private void Start()
    {
        UIManager.ShowNothing();
    }

    private void Update()
    {
        if (_currentWeapons.Count == 0)
            return;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var weapon = _currentWeapons[_currentWeaponIndex];
            weapon.Use(transform);
            if (weapon.amount <= 0)
            {
                _currentWeapons.Remove(weapon);
                _currentWeaponIndex = 0;
            }
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            _currentWeaponIndex--;
            if (_currentWeaponIndex < 0)
                _currentWeaponIndex = _currentWeapons.Count - 1;
            UpdateUI();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            _currentWeaponIndex++;
            if (_currentWeaponIndex >= _currentWeapons.Count)
                _currentWeaponIndex = 0;
            UpdateUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Weapon>())
        {
            var weapon = other.GetComponent<Weapon>();
            weapon.gameObject.SetActive(false);
            if (_currentWeapons.Any(w => w.displayName == weapon.displayName))
                _currentWeapons.First(w => w.displayName == weapon.displayName).IncreaseAmount();
            else
                _currentWeapons.Add(weapon);
                
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if(_currentWeapons.Count > 0)
            UIManager.ShowCurrentWeapon(_currentWeapons[_currentWeaponIndex]);
        else
            UIManager.ShowNothing();
    }
}