using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public IEnumerator Pop()
    {
        yield return new WaitForSeconds(1.0f);
        gameObject.SetActive(false);
    }
}
