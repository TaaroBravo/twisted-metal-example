using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI displayName;
    public TextMeshProUGUI displayAmount;
    public Image icon;

    public void ShowCurrentWeapon(Weapon weapon)
    {
        displayName.text = weapon.displayName;
        displayAmount.text = weapon.amount.ToString();
        icon.gameObject.SetActive(true);
        icon.sprite = weapon.icon;
    }

    public void ShowNothing()
    {
        icon.gameObject.SetActive(false);
        displayName.text = "";
        displayAmount.text = "";
    }
}