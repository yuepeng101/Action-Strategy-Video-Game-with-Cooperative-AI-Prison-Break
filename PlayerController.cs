using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    PlayerCharacter m_player;

	void Start ()
    {
        m_player = GetComponent<PlayerCharacter>();
	}

    void FixedUpdate()
    {
        // Let HP slider face towards screen
        m_player.healthSlider.transform.forward = Camera.main.transform.forward;

        if (m_player.healthSlider != null)
        {
            m_player.healthSlider.value = m_player.health;
        }

        if (Input.GetButtonDown("Fire1") && m_player.animator.GetFloat("Speed") < 0.1f)
        {
            m_player.Fire();
        }

        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");

        m_player.Move(new Vector3(h, 0, v));
        var lookDir = Vector3.forward * v + Vector3.right * h;
        if (lookDir.magnitude != 0)
        {
            m_player.Rotate(lookDir);
        }
    }

}
