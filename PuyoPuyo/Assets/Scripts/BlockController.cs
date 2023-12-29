using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    private float fallTimerMax = 0.5f;
    private float fallTimer;

    [SerializeField] GameManager gameManager;

    public void Init(GameManager manager , float timerMax)
    {
        gameManager  = manager;
        fallTimerMax = timerMax;
    }

    private void Start( )
    {
        fallTimer = fallTimerMax;
    }

    private void Update( )
    {
        fallTimer -= Time.deltaTime;


        Vector3 movePosition = Vector3.zero;

        if(Input.GetKeyUp(KeyCode.LeftArrow))
        {
            movePosition = Vector3.left;
        }

        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            movePosition = Vector3.right;
        }

        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            movePosition = Vector3.down;
        }
        else if(Input.GetKeyUp(KeyCode.UpArrow))
        {
            transform.Rotate(new Vector3(0,0,1) , 90);
            
            if (!gameManager.IsMovable(transform))
            {
                transform.Rotate(new Vector3(0, 0, 1), -90);
            }
        }

        if(fallTimer < 0 )
        {
            movePosition = Vector3.down;
            fallTimer = fallTimerMax;
        }

        transform.position += movePosition;

        //ˆÚ“®‚Å‚«‚È‚©‚Á‚½ê‡
        if(!gameManager.IsMovable(transform))
        {
            //Œ³‚É–ß‚·
            transform.position -= movePosition;

            //‰º‚ÉˆÚ“®‚Å‚«‚È‚©‚Á‚½ê‡
            if(movePosition == Vector3.down)
            {
                enabled = false;
            }
        }

    }

}
