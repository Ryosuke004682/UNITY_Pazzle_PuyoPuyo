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


    //ゲームの状態を定義
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

        //最初は生成済みなので、移動状態からスタート
        gameState = GameState.Move;

    }

    private void Update( )
    {
        //経過時間
        currentGameTime += Time.deltaTime;

        if (gameState == GameState.Move)
        {
            if (currentBlock.enabled) return;

            foreach (Transform item in currentBlock.transform)
            {
                Vector2Int index = GetIndexPosition(item.position);
                fieldTiles[index.x, index.y] = item;

                //ゲームオーバー
                if (index.y > FIELD_HEIGHT - 2)
                {
                    panelResult.SetActive(true);
                    enabled = false;
                }
            }

            gameState = GameState.Fall;
        }
        //落下
        else if (gameState == GameState.Fall)
        {
            fallTimer -= Time.deltaTime;
            if (0 < fallTimer) return;


            bool isFall = FallItem();

            //落下中
            if (isFall)
            {
                fallTimer = 0.1f;
            }
            //落ち切ったら削除ステートに移行
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
    /// ワールド座標をインデックス座標に変換
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
    /// 移動可能かどうか
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
    /// 次のブロックの作成
    /// </summary>
    private void SetupNextBlock( )
    {

        int randomCreate = Random.Range(0, prefabBlocks.Count);

        Vector3 setupPosition = new Vector3(2.5f, 11.0f, 0.0f);

        //ブロック生成
        BlockController prefab = prefabBlocks[randomCreate];
        nextBlock = Instantiate(prefab, setupPosition, Quaternion.identity);

        //時間経過によって落下速度を速くする。
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
        nextBlock.enabled = false;//動かないように固定
    }

    /// <summary>
    /// ブロックをフィールドへ
    /// </summary>
    private void SpawnBlock( )
    {
        Vector3 spawnPosition = new Vector3(0.5f, 8.5f, 0.0f);

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
    /// 削除可能かどうか
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
    /// リトライボタンの作成
    /// </summary>
    public void OnClickRetry( )
    {
        SceneManager.LoadScene("PuyoPuyoScene");
    }


    /// <summary>
    /// 一段落下
    /// </summary>
    /// <returns></returns>
    private bool FallItem( )
    {
        bool isFall = false;

        for (int y = 1; y < FIELD_HEIGHT; y++)
        {
            for (int x = 0; x < FIELD_WIDTH; x++)
            {
                //データ無し
                if (!fieldTiles[x, y]) continue;
                if (fieldTiles[x, y - 1]) continue;

                //座標の更新
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

        //現在のインデックスから4方向を調べる
        List<Vector2Int> direction = new List<Vector2Int>()
        {
            new Vector2Int(index.x - 1 , index.y),
            new Vector2Int(index.x + 1 , index.y),
            new Vector2Int(index.x , index.y - 1),
            new Vector2Int(index.x , index.y + 1),
        };

        foreach (var item in direction)
        {
            //アイテムがない、違う色はスキップする

            Transform tile = GetFieldTile(item);

            if (!tile) continue;

            Sprite sprite = tile.GetComponent<SpriteRenderer>().sprite;

            if (mainSprite != sprite) continue;

            //ここまで来たらアイテムを追加
            returnList.Add(item);
        }
        return returnList;
    }


    /// <summary>
    /// 指定されたインデックスのオブジェクトを削除
    /// </summary>
    /// <param name="list"></param>
    void DeleteItems( List<Vector2Int> list )
    {
        foreach (var item in list)
        {
            Destroy(fieldTiles[item.x, item.y].gameObject);
            fieldTiles[item.x, item.y] = null;

            //スコアを加算
            currentScore += SCORE_LINE;
            textScore.text = "" + currentScore;
         }
    }


    /// <summary>
    /// 同じ色のアイテムを削除する
    /// </summary>
    /// <returns></returns>
    private bool CheckItems()
    {
        //一度でも削除がされたかどうか
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

                //削除
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
