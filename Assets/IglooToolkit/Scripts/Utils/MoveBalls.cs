using UnityEngine;
using System.Collections;
using System;

public class MoveBalls : MonoBehaviour {
	private const float DEFAULT_SCALE = 0.2f;

	public float amount = 1.0f;
    public float offsetY = 0.0f;
    public float spinSpeed = 1.0f;

	public void setOffsetY(float y) { offsetY = y; }
	public void setSpinSpeed(float x) { spinSpeed = x; }
	public void setSpinSpeedNormalized(float x) { spinSpeed = 100 * Mathf.Max(0.0f,Mathf.Min(1.0f, x)); }
	public void setSphereShapes(int type)
	{
		Vector3 newScale;
		switch (type)
		{
			case 0:
				newScale = new Vector3(DEFAULT_SCALE, DEFAULT_SCALE, DEFAULT_SCALE);
				break;
			case 1:
				newScale = new Vector3(DEFAULT_SCALE, 100.0f, DEFAULT_SCALE);
				break;
			case 2:
				newScale = new Vector3(0.001f, DEFAULT_SCALE, DEFAULT_SCALE);
				break;
			default:
				newScale = new Vector3(DEFAULT_SCALE, DEFAULT_SCALE, DEFAULT_SCALE);
				break;
		}
		
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.transform.localScale = newScale;
		}
	}
	void Update () {
		Vector3 position = transform.position;
		position.y = Mathf.Sin (Time.time*3) * amount + offsetY;
		transform.position = position;
		transform.Rotate (0, spinSpeed * Time.deltaTime, 0);
	}
}
