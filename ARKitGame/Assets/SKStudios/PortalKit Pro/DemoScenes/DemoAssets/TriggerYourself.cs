using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerYourself : MonoBehaviour {
    public void Trigger() {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
