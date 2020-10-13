using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float xOffset = 7;
    float speed = 5;

    private void Update()
    {
        float moveHor = Input.GetAxis("Horizontal");
        float moveVert = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHor, 0, moveVert);

        transform.localPosition += movement * speed * Time.deltaTime;
    }

    public void SnapToPosition(Vector3 _pos)
    {
        transform.localPosition = new Vector3(_pos.x, transform.localPosition.y, _pos.z - xOffset);
    }
}
