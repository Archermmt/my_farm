using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour {
    [SerializeField] private Slider healthBar_;
    [SerializeField] private Slider energyBar_;
    [SerializeField] private TextMeshProUGUI moneyDes_;
    [SerializeField] private int money_ = 500;
    [SerializeField] private int healthMax_ = 100;
    [SerializeField] private int energyMax_ = 100;
    private int health_;
    private int energy_;

    private void Awake() {
        Reset();
        ShowMoney();
    }

    public void Reset() {
        health_ = healthMax_;
        energy_ = energyMax_;
        ShowHealth();
        ShowEnergy();
    }

    public int ChangeHealth(int delta) {
        health_ += delta;
        health_ = Math.Min(Math.Max(health_, 0), 100);
        ShowHealth();
        return health_;
    }

    public void ChangeHealthMax(int value) {
        healthMax_ = value;
    }

    public int ChangeEnergy(int delta) {
        energy_ += delta;
        energy_ = Math.Min(Math.Max(energy_, 0), 100);
        ShowEnergy();
        return energy_;
    }

    public void ChangeEnergyMax(int value) {
        energyMax_ = value;
    }

    public int ChangeMoney(int delta) {
        money_ += delta;
        ShowMoney();
        return money_;
    }

    private void ShowMoney() {
        moneyDes_.text = "G: " + money_;
    }

    private void ShowHealth() {
        healthBar_.value = (float)health_ / healthMax_;
    }

    private void ShowEnergy() {
        energyBar_.value = (float)energy_ / energyMax_;
    }

    public int health { get { return health_; } }
    public int energy { get { return energy_; } }
    public int money { get { return money_; } }
}
