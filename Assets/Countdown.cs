using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    TextMeshPro text;

    public void Start()
    {
        text = GetComponent<TextMeshPro>();
        text.enabled = false;
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownTimer());
    }

    IEnumerator CountdownTimer()
    {
        text.enabled = true;

        text.text = "3";
        yield return new WaitForSeconds(1);

        text.text = "2";
        yield return new WaitForSeconds(1);

        text.text = "1";
        yield return new WaitForSeconds(1);

        text.enabled = false;
    }
}
