using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInputMap", menuName = "Data/Input/PlayerInputMap")]
public class PlayerInputMap : ScriptableObject
{
    public List<PlayerActionBinding> actionBindings;
}