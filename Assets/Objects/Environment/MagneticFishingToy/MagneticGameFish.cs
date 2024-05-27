using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticGameFish : MonoBehaviour
{
    [SerializeField] AnimationCurve animationCurve;
    [SerializeField] float maxHeight;
    [SerializeField] float scaleSpeed;
    [SerializeField] float topJawMaxTurn;
    [SerializeField] float bottomJawMaxTurn;
    private float timeElapsed;
    private Vector3 initialPosition;
    private Transform topJaw;
    private Vector3 initialTopJawRotation;
    private Transform bottomJaw;
    private Vector3 initialBottomJawRotation;
    private bool isCaught = false;

    // Start is called before the first frame update
    void Start()
    {
        topJaw = transform.Find("FishTopJaw");
        bottomJaw = transform.Find("FishBottomJaw");
        
        // Randomness on start position
        timeElapsed = Random.Range(0.0f, 1.0f);
        initialPosition = transform.position;
        // Randomness of speed
        scaleSpeed += Random.Range(-0.3f, 0.5f);

        initialTopJawRotation = topJaw.transform.eulerAngles;
        initialBottomJawRotation = bottomJaw.transform.eulerAngles;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isCaught) {
            transform.position = new Vector3(
                transform.position.x, 
                initialPosition.y + animationCurve.Evaluate(timeElapsed) * maxHeight, 
                transform.position.z);

            topJaw.transform.eulerAngles = new Vector3(
                initialTopJawRotation.x - animationCurve.Evaluate(timeElapsed) * topJawMaxTurn,
                topJaw.transform.eulerAngles.y,
                topJaw.transform.eulerAngles.z
            );

            bottomJaw.transform.eulerAngles = new Vector3(
                initialBottomJawRotation.x + animationCurve.Evaluate(timeElapsed) * bottomJawMaxTurn,
                bottomJaw.transform.eulerAngles.y,
                bottomJaw.transform.eulerAngles.z
            );

            timeElapsed += Time.deltaTime * scaleSpeed;
            timeElapsed = timeElapsed % 1.0f;
        }
    }

    void OnTriggerEnter(Collider collider) {
        // Can only catch when fish's mouth is open
        if (collider.gameObject.tag == "MagnetFishingGameRod" && timeElapsed > 0.3f && timeElapsed < 0.73f) {
            transform.parent = collider.gameObject.transform;
            isCaught = true;
        }
    }
}
