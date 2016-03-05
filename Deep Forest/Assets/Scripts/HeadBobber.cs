using UnityEngine;
using System.Collections;

public class HeadBobber : MonoBehaviour {

    public float bobbingSpeed = 0.18f;
    public float bobbingAmount = 0.2f;
    float timer = 0;

    Vector3 pos;

    void Start()
    {
        pos = transform.localPosition;
    }

	void Update () {
        float waveSlice = 0;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
        {
            timer = 0;
        }
        else
        {
            waveSlice = Mathf.Sin(timer);
            timer += bobbingSpeed;
            if (timer > Mathf.PI * 2)
            {
                timer -= Mathf.PI * 2;
            }
        }
        if (waveSlice != 0)
        {
            float translateChange = waveSlice * bobbingAmount;
            float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            totalAxes = Mathf.Clamp01(totalAxes);
            translateChange = totalAxes * translateChange;
            pos.y = translateChange;
            transform.localPosition = pos;
        }
        else
        {
            pos.y = 0;
            transform.localPosition = pos;
        }
    }
}
