using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDataSO : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _maxStamina;

    private float _currentHealth;
    private float _currentStamina;

    public string Name { get => _name; protected set => _name = value; }
    public float MaxHealth { get => _maxHealth; protected set => _maxHealth = value; }
    public float MaxStamina { get => _maxStamina; protected set => _maxStamina = value; }
    public float CurrentHealth { get => _currentHealth; protected set => _currentHealth = value; }
    public float CurrentStamina { get => _currentStamina; protected set => _currentStamina = value; }

    public void HealthHandler(float value)
    {
        _currentHealth += value;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, 100);

        if (_currentHealth <= 0)
        {
            Debug.LogError("Dead");
        }
    }

    public void StaminaHandler(float value)
    {
        _currentStamina += value;
        _currentStamina = Mathf.Clamp(_currentStamina, 0, 100);

    }
}
