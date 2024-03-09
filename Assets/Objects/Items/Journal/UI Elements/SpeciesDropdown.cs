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

	bool isOpen = false;
	bool isInteractable = true;
	RectTransform dropdownTransform;

	void Awake()
	{
		UpdateTextFields();
	}

	void Start()
	{
		dropdownTransform = dropdownPanel.GetComponent<RectTransform>();
	}

	//Implemented by IPointerClickHandler interface
	public override void OnPointerClick(PointerEventData eventData)
	{
		if (!isInteractable) return;

		if (!isOpen)
		{
			OpenDropdown();
		}
		else
		{
			CloseDropdown();
		}
	}

	//Inherited from JournalUIElement
	public override void Reset()
	{
		dropdownTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
		isOpen = false;
		isInteractable = true;
	}

	//Opens the species dropdown and displays the fish's data
	public void OpenDropdown()
	{
		isOpen = true;
		StartCoroutine(DoOpenDropdown());
	}

	public void CloseDropdown()
	{
		isOpen = false;
		StartCoroutine(DoCloseDropdown());
	}

	public void SetFishSO(FishSO fishSO)
	{
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
	}
}
