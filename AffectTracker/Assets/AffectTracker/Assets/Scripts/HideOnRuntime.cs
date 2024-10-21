using UnityEngine;

public class HideOnRuntime : MonoBehaviour {
	void Start() {
		this.gameObject.SetActive(false);
	}
}
