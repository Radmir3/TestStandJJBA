using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    [SerializeField] Animator anim;
    PlayerController isHolding;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void dummy()
    {
        anim.SetBool("attack", true);
        Invoke("stopAttack", 0.5f);
    }

    public void startbarrage()
    {
        anim.SetTrigger("Barrage");
    }

    private void stopAttack()
    {
        anim.SetBool("Barrage", false);
        anim.SetBool("attack", false);
    }
}
