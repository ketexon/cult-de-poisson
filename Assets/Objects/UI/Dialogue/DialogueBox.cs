using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

public class DialogueBox : MonoBehaviour
{
    [SerializeField] TMP_Text textComponent;


    Player player;

    void Start()
    {
        player = Player.Instance;
    }

    void Update()
    {
        LookAtPlayer();
    }

    public void UpdateText(string text)
    {
        textComponent.text = "";
        StartCoroutine(DoUpdateText(text));
    }

    void LookAtPlayer()
    {
        Quaternion target = Quaternion.LookRotation(player.transform.position - transform.position);
        Quaternion current = transform.parent.rotation;

        float angle = Mathf.Max(Quaternion.Angle(current, target), 15f);

        transform.parent.rotation = Quaternion.RotateTowards(current, target, angle * Time.deltaTime);
    }

    IEnumerator DoUpdateText(string text)
    {
        foreach (char c in text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(0.05f);
        }
    }
}
