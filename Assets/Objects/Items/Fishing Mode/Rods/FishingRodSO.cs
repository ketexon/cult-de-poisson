using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Fishing Rod", menuName = "Fishing/Rod")]
public class FishingRodSO : ScriptableObject
{
    [SerializeField] public FishingRodV2 Prefab;
}