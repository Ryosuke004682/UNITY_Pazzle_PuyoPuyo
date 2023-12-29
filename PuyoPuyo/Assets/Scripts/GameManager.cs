using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    const int FIELD_WIDTH = 10;
    const int FIELD_HEIGHT = 20;
    const int SCORE_LINE = 100;

    [SerializeField] List<BlockController> prefabBlocks;

    //����̗������x
    [SerializeField] private float startFallTimerMax;

    [SerializeField] Text textScore;
    [SerializeField] GameObject panelResult;

    [SerializeField] AudioClip seHit;
    [SerializeField] AudioClip seDelete;

    BlockController nextBlock;
    BlockController currentBlock;

    Transform[,] fieldTiles;

    int currentScore;
    float currentGameTime;

    AudioSource audioSource;


    //�Q�[���̏�Ԃ��`
    enum GameState
    {
        Move,
        Fall,
        Delete
    }
    GameState gameState;


    float fallTimer;


    private void Start( )
    {
        fieldTiles = new Transform[FIELD_WIDTH, FIELD_HEIGHT];

        audioSource = GetComponent<AudioSource>();

        SetupNextBlock();
        SpawnBlock();

        currentScore = 0;
        textScore.text = "" + currentScore;

        panelResult.SetActive(false);

        //�ŏ��͐����ς݂Ȃ̂ŁA�ړ���Ԃ���X�^�[�g
        gameState = GameState.Move;

    }

    private void Update( )
    {
        //�o�ߎ���
        currentGameTime += Time.deltaTime;

        if (gameState == GameState.Move)
        {
            if (currentBlock.enabled) return;

            foreach (Transform item in currentBlock.transform)
            {
                Vector2Int index = GetIndexPosition(item.position);
                fieldTiles[index.x, index.y] = item;

                //�Q�[���I�[�o�[
                if (index.y > FIELD_HEIGHT - 2)
                {
                    panelResult.SetActive(true);
                    enabled = false;
                }
            }

            gameState = GameState.Fall;
        }
        //����
        else if (gameState == GameState.Fall)
        {
            fallTimer -= Time.deltaTime;
            if (0 < fallTimer) return;


            bool isFall = FallItem();

            //������
            if (isFall)
            {
                fallTimer = 0.1f;
            }
            //�����؂�����폜�X�e�[�g�Ɉڍs
            else
            {
                audioSource.PlayOneShot(seHit);
                gameState = GameState.Delete;
            }
        }
        else if (gameState == GameState.Delete)
        {
            bool isDelete = CheckItems();

            if(isDelete)
            {
                audioSource.PlayOneShot(seDelete);
                gameState = GameState.Fall;
            }
            else
            {
                SpawnBlock();
                gameState = GameState.Move;
            }
        }
    }


    /// <summary>
    /// ���[���h���W���C���f�b�N�X���W�ɕϊ�
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    Vector2Int GetIndexPosition( Vector3 position )
    {
        Vector2Int index = new Vector2Int();

        index.x = Mathf.RoundToInt(position.x - 0.5f) + FIELD_WIDTH / 2;
        index.y = Mathf.RoundToInt(position.y - 0.5f) + FIELD_HEIGHT / 2;

        return index;
    }


    /// <summary>
    /// �ړ��\���ǂ���
    /// </summary>
    /// <param name="blockTransform"></param>
    /// <returns></returns>
    public bool IsMovable( Transform blockTransform )
    {
        foreach (Transform item in blockTransform)
        {
            Vector2Int index = GetIndexPosition(item.position);

            if (index.x < 0 || FIELD_WIDTH - 1 < index.x || index.y < 0)
            {
                return false;
            }

            if (GetFieldTile(index))
            {
                return false;
            }
        }
        return true;
    }


    /// <summary>
    /// ���̃u���b�N�̍쐬
    /// </summary>
    private void SetupNextBlock( )
    {

        int randomCreate = Random.Range(0, prefabBlocks.Count);

        Vector3 setupPosition = new Vector3(2.5f, 11.0f, 0.0f);

        //�u���b�N����
        BlockController prefab = prefabBlocks[randomCreate];
        nextBlock = Instantiate(prefab, setupPosition, Quaternion.identity);

        //���Ԍo�߂ɂ���ė������x�𑬂�����B
        float fallTime = startFallTimerMax;

        if (currentGameTime > 50)
        {
            fallTime = startFallTimerMax * 0.1f;
        }
        else if (currentGameTime > 30)
        {
            fallTime = startFallTimerMax * 0.3f;
        }
        else if (currentGameTime > 5)
        {
            fallTime = startFallTimerMax * 0.4f;
        }


        nextBlock.Init(this, fallTime);
        nextBlock.enabled = false;//�����Ȃ��悤�ɌŒ�
    }

    /// <summary>
    /// �u���b�N���t�B�[���h��
    /// </summary>
    private void SpawnBlock( )
    {
        Vector3 spawnPosition = new Vector3(0.5f, 8.5f, 0.0f);

        //�u���b�N���Z�b�g
        currentBlock = nextBlock;
        currentBlock.transform.position = spawnPosition;

        currentBlock.enabled = true;

        SetupNextBlock();//���̃u���b�N���Z�b�g
    }


    /// <summary>
    /// �t�B�[���h�̃u���b�N������
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Transform GetFieldTile( Vector2Int index )
    {
        if (index.x < 0 || FIELD_WIDTH - 1 < index.x ||
            index.y < 0 || FIELD_HEIGHT - 1 < index.y)
        {
            return null;
        }

        return fieldTiles[index.x, index.y];
    }


    /// <summary>
    /// �폜�\���ǂ���
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    bool IsDeleteLine( int y )
    {
        for (int x = 0; x < FIELD_WIDTH; x++)
        {
            if (!fieldTiles[x, y]) return false;
        }

        return true;
    }

    /// <summary>
    /// ���g���C�{�^���̍쐬
    /// </summary>
    public void OnClickRetry( )
    {
        SceneManager.LoadScene("PuyoPuyoScene");
    }


    /// <summary>
    /// ��i����
    /// </summary>
    /// <returns></returns>
    private bool FallItem( )
    {
        bool isFall = false;

        for (int y = 1; y < FIELD_HEIGHT; y++)
        {
            for (int x = 0; x < FIELD_WIDTH; x++)
            {
                //�f�[�^����
                if (!fieldTiles[x, y]) continue;
                if (fieldTiles[x, y - 1]) continue;

                //���W�̍X�V
                fieldTiles[x, y].position += Vector3.down;

                fieldTiles[x, y - 1] = fieldTiles[x, y];
                fieldTiles[x, y] = null;

                isFall = true;
            }
        }
        return isFall;
    }


    private List<Vector2Int> GetSameItems( Vector2Int index )
    {
        List<Vector2Int> returnList = new List<Vector2Int>();

        Sprite mainSprite = fieldTiles[index.x, index.y].GetComponent<SpriteRenderer>().sprite;

        //���݂̃C���f�b�N�X����4�����𒲂ׂ�
        List<Vector2Int> direction = new List<Vector2Int>()
        {
            new Vector2Int(index.x - 1 , index.y),
            new Vector2Int(index.x + 1 , index.y),
            new Vector2Int(index.x , index.y - 1),
            new Vector2Int(index.x , index.y + 1),
        };

        foreach (var item in direction)
        {
            //�A�C�e�����Ȃ��A�Ⴄ�F�̓X�L�b�v����

            Transform tile = GetFieldTile(item);

            if (!tile) continue;

            Sprite sprite = tile.GetComponent<SpriteRenderer>().sprite;

            if (mainSprite != sprite) continue;

            //�����܂ŗ�����A�C�e����ǉ�
            returnList.Add(item);
        }
        return returnList;
    }


    /// <summary>
    /// �w�肳�ꂽ�C���f�b�N�X�̃I�u�W�F�N�g���폜
    /// </summary>
    /// <param name="list"></param>
    void DeleteItems( List<Vector2Int> list )
    {
        foreach (var item in list)
        {
            Destroy(fieldTiles[item.x, item.y].gameObject);
            fieldTiles[item.x, item.y] = null;

            //�X�R�A�����Z
            currentScore += SCORE_LINE;
            textScore.text = "" + currentScore;
         }
    }


    /// <summary>
    /// �����F�̃A�C�e�����폜����
    /// </summary>
    /// <returns></returns>
    private bool CheckItems()
    {
        //��x�ł��폜�����ꂽ���ǂ���
        bool isDelete = false;

        for(int x = 0; x < FIELD_WIDTH; x++)
        {
            for(int y = 0; y < FIELD_HEIGHT; y++)
            {
                if (!fieldTiles[x, y]) continue;

                List<Vector2Int> checkItems = new List<Vector2Int>();
                checkItems.Add(new Vector2Int(x, y));

                int checkIndex = 0;

                while(checkIndex < checkItems.Count)
                {
                    List<Vector2Int> sameItems = GetSameItems(checkItems[checkIndex]);

                    checkIndex++;


                    foreach(var items in sameItems)
                    {
                        if (checkItems.Contains(items)) continue;

                        checkItems.Add(items);
                    }
                }

                //�폜
                if(checkItems.Count > 3)
                {
                    DeleteItems(checkItems);
                    isDelete = true;

                }
            }
        }

        return isDelete;

    }

}
