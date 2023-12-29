using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{

    [SerializeField] List<Sprite> spritePuyos;

    private float fallTimerMax = 0.5f;
    private float fallTimer;

    [SerializeField] GameManager gameManager;

    public void Init(GameManager manager , float timerMax)
    {
        gameManager  = manager;
        fallTimerMax = timerMax;

        //子オブジェクトの画像をセット
        foreach(Transform item in transform)
        {
            //ランダムな画像を持ってくる
            int random = Random.Range(0 , spritePuyos.Count);
            Sprite sprite = spritePuyos[random];

            item.GetComponent<SpriteRenderer>().sprite = sprite;
         }

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
            
            //顔も回転させる
            foreach(Transform item in transform)
            {
                item.transform.Rotate(new Vector3(0, 0, 1), -90);
            }

            if (!gameManager.IsMovable(transform))
            {
                transform.Rotate(new Vector3(0, 0, 1), -90);
                foreach (Transform item in transform)
                {
                    item.transform.Rotate(new Vector3(0, 0, 1), 90);
                }
            }
        }

        if(fallTimer < 0 )
        {
            movePosition = Vector3.down;
            fallTimer = fallTimerMax;
        }

        transform.position += movePosition;

        //移動できなかった場合
        if(!gameManager.IsMovable(transform))
        {
            //元に戻す
            transform.position -= movePosition;

            //下に移動できなかった場合
            if(movePosition == Vector3.down)
            {
                enabled = false;
            }
        }

    }

}
