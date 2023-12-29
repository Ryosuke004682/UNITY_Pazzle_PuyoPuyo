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

        //�q�I�u�W�F�N�g�̉摜���Z�b�g
        foreach(Transform item in transform)
        {
            //�����_���ȉ摜�������Ă���
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
            
            //�����]������
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

        //�ړ��ł��Ȃ������ꍇ
        if(!gameManager.IsMovable(transform))
        {
            //���ɖ߂�
            transform.position -= movePosition;

            //���Ɉړ��ł��Ȃ������ꍇ
            if(movePosition == Vector3.down)
            {
                enabled = false;
            }
        }

    }

}
