using UnityEngine;

public class BoarAnim : MonoBehaviour
{
    public Animator anim;
    
    private int idleLoops = 0;
    private int alertLoops = 0;

    private bool isAlert = false;
    private float lastNormalizedTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

        float normalizedTime = state.normalizedTime % 1f;

        if (normalizedTime < lastNormalizedTime)
        {
            if (!isAlert && state.IsName("BoarIdle"))
            {
                idleLoops++;
                if (idleLoops >= 5)
                {
                    anim.SetBool("Alert", true);
                    isAlert = true;
                    idleLoops = 0;
                }
            }
            else if (isAlert && state.IsName("BoarAlert"))
            {
                alertLoops++;
                if (alertLoops >= 5)
                {
                    anim.SetBool("Alert", false);
                    isAlert = false;
                    alertLoops = 0;
                }
            }
        }

        lastNormalizedTime = normalizedTime;
    }

    void PlayAlert()
    {
        anim.SetBool("Alert", true);
    }
}
