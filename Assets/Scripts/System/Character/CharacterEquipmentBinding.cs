using System;
using UnityEngine;

// Connects a live CharacterStats to session equipment. Also used by combat actors before activation.
[DisallowMultipleComponent]
public sealed class CharacterEquipmentBinding : MonoBehaviour
{
    private CharacterStats stats;
    private string characterId;
    private CharacterEquipment subscribed;
    public void Initialize(CharacterStats target, string id)
    {
        Unsubscribe(); stats = target; characterId = id;
        if (stats == null || string.IsNullOrEmpty(id)) throw new ArgumentException("Stats and a registered character ID are required.");
        Apply(); if (isActiveAndEnabled) Subscribe();
    }
    private void OnEnable() { if (stats != null) { Subscribe(); Apply(); } }
    private void OnDisable() => Unsubscribe();
    private void Subscribe()
    {
        subscribed = InventorySession.Equipment; subscribed.Changed += Apply;
    }
    private void Unsubscribe() { if (subscribed != null) subscribed.Changed -= Apply; subscribed = null; }
    private void Apply() { if (stats != null) EquipmentStatEvaluator.Apply(stats, InventorySession.Equipment.GetEquipped(characterId)); }
}
