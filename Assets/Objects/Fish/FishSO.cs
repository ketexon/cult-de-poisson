using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Fish", menuName = "Fish")]
public class FishSO : ScriptableObject
{
    public string Name;
    public GameObject InWaterPrefab;
    public GameObject InHandPrefab;
    public GameObject InBucketPrefab;
    public GameObject PhysicalPrefab;
	public FishInfo FishInfo;
}


[Serializable]
public struct FishInfo
{
	public enum Depth { Shallow, Mid, Deep }

	public float Weight;
	[Min(0)] public int LengthFeet, LengthInches;
	public Depth FishDepth;
	public string CatchMethod;
	public string FavoredHabitat;
	public int CatchDifficulty;
	public bool IsKeyFish;
	public string Notes;
}
