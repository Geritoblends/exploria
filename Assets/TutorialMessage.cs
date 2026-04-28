using UnityEngine;
using System.Collections;

public class TutorialMessage : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float duracion = 3f;

    void Start()
    {
        if (PlayerPrefs.GetInt("TutorialMostrado", 0) == 0)
        {
            StartCoroutine(Mostrar());
        }
        else
        {
            canvasGroup.alpha = 0;
        }
    }

    IEnumerator Mostrar()
    {
        PlayerPrefs.SetInt("TutorialMostrado", 1);

        // Fade in del mensaje
        for (float i = 0; i <= 1; i += Time.deltaTime)
        {
            canvasGroup.alpha = i;
            yield return null;
        }

        yield return new WaitForSeconds(duracion);

        // Fade out del mensaje
        for (float i = 1; i >= 0; i -= Time.deltaTime)
        {
            canvasGroup.alpha = i;
            yield return null;
        }
    }
}