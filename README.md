# Twisted Metal–Style Weapons (Unity)

[![Unity](https://img.shields.io/badge/Unity-2021%2B-black?logo=unity&logoColor=white)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-7.0%2B-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Design Pattern](https://img.shields.io/badge/Pattern-Strategy-blueviolet)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Project Type](https://img.shields.io/badge/Type-Gameplay%20Prototype-blue)](#)
[![Made With ❤️](https://img.shields.io/badge/Made%20with-%E2%9D%A4-red)](#)
[![Last Commit](https://img.shields.io/github/last-commit/TaaroBravo/twisted-metal-example)](https://github.com/TaaroBravo/twisted-metal-example/commits/main)

Small Unity prototype inspired by *Twisted Metal*.
The focus is the **Weapon System** implemented with a **Strategy pattern** (polymorphism) so you can add new weapons without changing existing code.

## Features

* **Strategy-based weapons**: a common `Weapon` abstraction with concrete weapons like `FireWeapon` and `FrostWeapon`.
* **Plug-and-play**: create a new weapon by subclassing `Weapon` and assigning an icon, name, amount and a bullet prefab.
* **UI binding**: minimal `UIManager` to show current weapon name, icon, and remaining ammo.
* **Simple vehicle controller** to move the car while testing weapons.
* **Clean separation of concerns**: `WeaponSystem` handles selection/usage, each `Weapon` handles how to fire.

## Project Structure (scripts)

```
Scripts/
 ├─ Weapon.cs              # Base class (Strategy): name, amount, icon, bullet prefab; Use() & IncreaseAmount()
 ├─ FireWeapon.cs          # Concrete strategy (inherits Weapon)
 ├─ FrostWeapon.cs         # Concrete strategy (inherits Weapon)
 ├─ WeaponSystem.cs        # Manages current weapons, cycling, using, adding, UI updates
 ├─ Bullet.cs              # Moves forward; used by Weapon prefabs
 ├─ SimpleCarController.cs # WASD movement for quick testing
 └─ UIManager.cs           # Shows weapon name, ammo, icon
```

## Design: Strategy Pattern

The **Strategy** is the `Weapon` base class:

```csharp
public abstract class Weapon : MonoBehaviour
{
    public string displayName;
    public int amount;      // ammo/charges
    public Sprite icon;
    public Bullet prefab;   // projectile to spawn

    int _initialAmount;

    void Start() => _initialAmount = amount;

    public virtual void Use(Transform player)
    {
        amount--;
        var bullet = Instantiate(prefab);
        bullet.transform.position = player.position + Vector3.up;
        bullet.transform.forward  = player.forward;
    }

    public void IncreaseAmount() => amount += _initialAmount;
}
```

Concrete weapons (strategies) are simple subclasses (override `Use` if behavior differs):

```csharp
public class FireWeapon : Weapon { /* override Use() to add fire effects, AoE, etc. */ }
public class FrostWeapon : Weapon { /* override Use() to add slow/freeze, etc. */ }
```

`WeaponSystem` owns the current list and delegates to the active strategy:

```csharp
public class WeaponSystem : MonoBehaviour
{
    public UIManager UIManager;

    List<Weapon> _currentWeapons = new();
    int _currentWeaponIndex;

    void Start() => UIManager.ShowNothing();

    // Call this when picking up a weapon
    public void AddWeapon(Weapon weapon)
    {
        var existing = _currentWeapons.FirstOrDefault(w => w.GetType() == weapon.GetType());
        if (existing != null) existing.IncreaseAmount();
        else _currentWeapons.Add(weapon);
        UpdateUI();
    }

    public void UseCurrent(Transform player)
    {
        if (_currentWeapons.Count == 0) return;
        _currentWeapons[_currentWeaponIndex].Use(player);
        if (_currentWeapons[_currentWeaponIndex].amount <= 0)
            _currentWeapons.RemoveAt(_currentWeaponIndex);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (_currentWeapons.Count > 0)
            UIManager.ShowCurrentWeapon(_currentWeapons[_currentWeaponIndex]);
        else
            UIManager.ShowNothing();
    }
}
```

## How to Run

1. Open the project in **Unity** (the sample scene is `Scenes/SampleScene.unity`).
2. Press **Play**.

   * Drive with **W / A / D** (`SimpleCarController`).
   * Hook your input to `WeaponSystem.UseCurrent()` and to cycle weapons if needed.
3. The **UI** updates automatically through `UIManager` when the current weapon changes or ammo runs out.

## ➕ Adding a New Weapon (in 60 seconds)

1. Create a **new script** that inherits from `Weapon` (e.g., `LaserWeapon.cs`).
2. (Optional) **Override** `Use(Transform player)` for custom behavior (spread, homing, slow, AoE, status effects, etc.).
3. Create a **Weapon prefab** with:

   * the new `Weapon` component,
   * an **icon**,
   * `displayName`,
   * initial `amount`,
   * a `Bullet` prefab (or your own projectile).
4. Call `WeaponSystem.AddWeapon(newWeaponPrefab)` when the player picks it up.

Because it’s Strategy-based, new weapons **don’t require changes** to `WeaponSystem`.

## UI

`UIManager` uses TextMeshPro and an `Image` to display:

* **Name** (`displayName`)
* **Ammo** (`amount`)
* **Icon** (`icon`)

Call `UIManager.ShowCurrentWeapon(weapon)` to refresh, or `ShowNothing()` when the player has none.

## Notes & Next Steps

* The `Bullet` script is intentionally minimal (`transform.forward * speed`). Extend it with damage, lifetime, VFX, hit detection, etc.
* Consider adding:

  * **Pickup trigger** that calls `WeaponSystem.AddWeapon(...)`
  * Input bindings for **cycle next/previous weapon**
  * **Cooldowns** / **charge types** per weapon
  * **Object pooling** for bullets (performance)
  * Distinct **VFX/SFX** per concrete weapon

---
