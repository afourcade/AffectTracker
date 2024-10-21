using UnityEngine;


public class HelpBoxAttribute : PropertyAttribute {

	public string text;

	public HelpBoxAttribute(string text) {
		this.text = text;
	}
}
