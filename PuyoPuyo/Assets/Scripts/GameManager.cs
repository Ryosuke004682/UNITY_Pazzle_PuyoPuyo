using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    const int FIELD_WIDTH  = 10;
    const int FIELD_HEIGHT = 20;
    const int SCORE_LINE   = 100;

    [SerializeField] List<BlockController> prefabBlocks;
    
    //初回の落下速度
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


    private void Start( )
    {
        fieldTiles = new Transform[FIELD_WIDTH, FIELD_HEIGHT];

        audioSource = GetComponent<AudioSource>();

        SetupNextBlock();
        SpawnBlock();

        currentScore = 0;
        textScore.text = "" + currentScore;

        panelResult.SetActive( false );
    }

    private void Update( )
    {
        //経過時間
        currentGameTime += Time.deltaTime;

        if (currentBlock.enabled) return;

        foreach(Transform item in currentBlock.transform)
        {
            Vector2Int index = GetIndexPosition(item.position);
            fieldTiles[index.x, index.y] = item;

            //ゲームオーバー
            if(index.y > FIELD_HEIGHT - 2)
            {
                panelResult.SetActive( true );
                enabled = false;
            }
        }

        DeleteLines();
        SpawnBlock();


        audioSource.PlayOneShot(seHit);

    }


    /// <summary>
    /// ワールド座標をインデックス座標に変換
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    Vector2Int GetIndexPosition(Vector3 position)
    {
        Vector2Int index = new Vector2Int();

        index.x = Mathf.RoundToInt(position.x - 0.5f) + FIELD_WIDTH  / 2;
        index.y = Mathf.RoundToInt(position.y - 0.5f) + FIELD_HEIGHT / 2;

        return index;
    }


    /// <summary>
    /// 移動可能かどうか
    /// </summary>
    /// <param name="blockTransform"></param>
    /// <returns></returns>
    public bool IsMovable(Transform blockTransform)
    {
        foreach(Transform item in blockTransform)
        {
            Vector2Int index = GetIndexPosition(item.position);

            if (index.x < 0 || FIELD_WIDTH - 1 < index.x || index.y < 0)
            {
                return false;
            }

            if(GetFieldTile(index))
            {
                return false;
            }
        }
        return true;
    }


    /// <summary>
    /// 次のブロックの作成
    /// </summary>
    private void SetupNextBlock()
    {

        int randomCreate = Random.Range(0 , prefabBlocks.Count);

        Vector3 setupPosition = new Vector3(2.5f , 11.0f , 0.0f);

        //ブロック生成
        BlockController prefab = prefabBlocks[randomCreate];
        nextBlock = Instantiate(prefab , setupPosition , Quaternion.identity);

        //時間経過によって落下速度を速くする。
        float fallTime = startFallTimerMax;
        
        if(currentGameTime > 50)
        {
            fallTime = startFallTimerMax * 0.1f;
        }
        else if(currentGameTime > 30)
        {
            fallTime = startFallTimerMax * 0.3f;
        }
        else if(currentGameTime > 5)
        {
            fallTime = startFallTimerMax * 0.4f;
        }


        nextBlock.Init(this , fallTime);
        nextBlock.enabled = false;//動かないように固定
    }

    /// <summary>
    /// ブロックをフィールドへ
    /// </summary>
    private void SpawnBlock()
    {
        Vector3 spawnPosition = new Vector3(0.5f , 8.5f , 0.0f);

        //ブロックをセット
        currentBlock = nextBlock;
        currentBlock.transform.position = spawnPosition;

        currentBlock.enabled = true;

        SetupNextBlock();//次のブロックをセット
    }


    /// <summary>
    /// フィールドのブロックを消す
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Transform GetFieldTile(Vector2Int index)
    {
        if(index.x < 0 || FIELD_WIDTH - 1 < index.x ||
            index.y < 0 || FIELD_HEIGHT - 1 < index.y)
        {
            return null;
        }

        return fieldTiles[index.x, index.y];
    }


    /// <summary>
    /// 削除可能かどうか
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    bool IsDeleteLine(int y)
    {
        for(int x = 0; x < FIELD_WIDTH; x++)
        {
            if (!fieldTiles[x, y]) return false;
        }

        return true;
    }

    /// <summary>
    /// ラインを削除
    /// </summary>
    private void DeleteLine(int y )
    {
        for(int x = 0; x < FIELD_WIDTH; x++)
        {
            Destroy(fieldTiles[x , y].gameObject);
        }
    }

    /// <summary>
    /// ラインを下げる
    /// </summary>
    /// <param name="startY"></param>
    void FallLine( int startY )
    {
        //指定したライン上全て
        for(int y = startY + 1; y < FIELD_HEIGHT; y++)
        {
            for(int x = 0; x < FIELD_WIDTH; x++)
            {
                if (!fieldTiles[x, y]) continue;

                //ワールド座標更新
                fieldTiles[x, y].position += Vector3.down;
                //内部データ更新
                fieldTiles[x, y - 1] = fieldTiles[x, y];
                fieldTiles[x, y]     = null;
            }
        }
    }


    /// <summary>
    /// ライン削除
    /// </summary>
    void DeleteLines()
    {
        bool isPlaySE = false;

        //上から調べる
        for(int y = FIELD_HEIGHT - 1; y >= 0; y--)
        {
            //ラインがそろってるかどうかのチェック
            if (!IsDeleteLine(y)) continue;

            DeleteLine(y);//ラインを削除
            FallLine(y)  ;//ラインを下に下げる

            currentScore += SCORE_LINE;
            textScore.text = "" + currentScore;

            isPlaySE = true;
        }

        if(isPlaySE)
        {
            audioSource.PlayOneShot(seDelete);
        }

    }

    /// <summary>
    /// リトライボタンの作成
    /// </summary>
    public void OnClickRetry()
    {
        SceneManager.LoadScene("TetrisScene");
    }

}
