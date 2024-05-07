using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class SpeciesDropdown : JournalUIElement
{
	const float DROPDOWN_HEIGHT = 360f;
	const float TOLERANCE = 10f;

	[SerializeField] FishSO fishSO;
	[SerializeField] TMP_Text speciesLabel;
	[SerializeField] GameObject dropdownPanel;
	[SerializeField] TMP_Text weight, length, depth, catchMethod, favoredHabitat, catchDifficulty, isKeyFish, notes;

	bool isInteractable = true;
	RectTransform dropdownTransform;

	void Start()
	{
		UpdateTextFields();
		dropdownTransform = dropdownPanel.GetComponent<RectTransform>();
	}

	//Inherited from JournalUIElement
	public override void Reset()
	{
		dropdownTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
		OpenDropdown();
	}

	//Opens the species dropdown and displays the fish's data
	public void OpenDropdown()
	{
		if (!isInteractable) return;
		StartCoroutine(DoOpenDropdown());
	}

	public void CloseDropdown()
	{
		if (!isInteractable) return;
		StartCoroutine(DoCloseDropdown());
	}

	public void SetFishSO(FishSO fishSO)
	{
		if (!isInteractable) return;
		this.fishSO = fishSO;
		UpdateTextFields();
	}

	IEnumerator DoOpenDropdown()
	{
		isInteractable = false;
		dropdownPanel.SetActive(true);
		yield return new WaitForSeconds(0.1f);

		float currentVelocity = 0f;
		while (dropdownTransform.rect.height < DROPDOWN_HEIGHT - TOLERANCE)
		{
			float curHeight = dropdownTransform.rect.height;
			curHeight = Mathf.SmoothDamp(curHeight, DROPDOWN_HEIGHT, ref currentVelocity, 0.4f);
			dropdownTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, curHeight);
			yield return null;
		}

		isInteractable = true;
	}

	IEnumerator DoCloseDropdown()
	{
		isInteractable = false;

		RectTransform rectTransform = dropdownPanel.GetComponent<RectTransform>();

		float currentVelocity = 0f;
		while (rectTransform.rect.height > 0 + TOLERANCE)
		{
			float curHeight = rectTransform.rect.height;
			curHeight = Mathf.SmoothDamp(curHeight, 0, ref currentVelocity, 0.4f);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, curHeight);
			yield return null;
		}

		dropdownPanel.SetActive(false);
		isInteractable = true;
	}

	void UpdateTextFields()
	{
		speciesLabel.text = fishSO.Name;
		weight.text = "Weight: " + fishSO.FishInfo.Weight + " lbs";
		length.text = "Length: " + fishSO.FishInfo.LengthFeet + "' " + fishSO.FishInfo.LengthInches + "\"";
		depth.text = "Depth: " + fishSO.FishInfo.FishDepth;
		catchMethod.text = "Catch Method: " + fishSO.FishInfo.CatchMethod;
		favoredHabitat.text = "Favored Habitat: " + fishSO.FishInfo.FavoredHabitat;
		catchDifficulty.text = "Catch Difficulty: " + fishSO.FishInfo.CatchDifficulty;
		isKeyFish.text = "Key Fish: " + fishSO.FishInfo.IsKeyFish;
		notes.text = "Notes: " + fishSO.FishInfo.Notes;
		StartCoroutine(CloseAndOpenDropdown());
	}

	IEnumerator CloseAndOpenDropdown()
	{
		CloseDropdown();
		yield return new WaitWhile(() => !isInteractable);
		OpenDropdown();
	}
}