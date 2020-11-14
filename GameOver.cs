using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOver : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var a = GameObject.Find("Body").GetComponent<Animator>();
        a.SetBool("Death_b", true);
        var scoreText =
            GameObject.Find("Score").GetComponent<TextMeshProUGUI>();
        scoreText.text = "You killed " + Menu.score + " dogs."; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
