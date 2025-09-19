using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tutorail : MonoBehaviour
{

    public GameObject MovePanel;
    public GameObject JumpPanel;
    bool hasMoved = false;
    bool hasJumped = false;
    float delayBeforeNextStep = 1f;
    float timer = 0f;


    // Start is called before the first frame update
    void Start()
    {

        MovePanel.SetActive(true);
        JumpPanel.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

        if (!hasMoved)
        {

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {

                hasMoved = true;
                MovePanel.SetActive(false);
                timer = delayBeforeNextStep;

            }

            return;

        }

        if (hasMoved && !hasJumped && timer > 0)
        {

            timer -= Time.deltaTime;

            if (timer <= 0)
            {

                JumpPanel.SetActive(true);

            }

        }

        if (hasMoved && !hasJumped)
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {

                hasJumped = true;
                JumpPanel.SetActive(false);

            }

        }

    }
}
