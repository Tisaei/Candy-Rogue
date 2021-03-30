using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using CandyRogueBase;

public class SequenceManagerController : MonoBehaviour
{
    [SerializeField]
    private GameObject NowLoading, Tilemap;
    private NowLoadingController nowLoadingController;
    private GameObject Player;
    private PlayerController playerController;
    private TilemapController tilemapController;
    private List<EnemyController> enemyControllers;

    public Status status { get; set; } = Status.KEY_INPUT;

    // Start is called before the first frame update
    void Start()
    {
        nowLoadingController = NowLoading.GetComponent<NowLoadingController>();
        Player = GameObject.FindWithTag("Player");
        playerController = Player.GetComponent<PlayerController>();
        tilemapController = Tilemap.GetComponent<TilemapController>();
        enemyControllers = new List<EnemyController>();
        enemyControllers.AddRange(Array.ConvertAll(GameObject.FindGameObjectsWithTag("Enemy"), g => g.GetComponent<EnemyController>()));
        tilemapController.RefCopyEnemyControllers(enemyControllers);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public enum Status
    {
        KEY_INPUT,

        MOVE_E_DECIDE,
        MOVE_PE_MOVE,
        MOVE_E_ACT,

        ACT_P_ACT,
        ACT_E_DECIDE,
        ACT_E_ACT,
        ACT_E_MOVE
    }

    public async UniTask DoOneTurn(Behavior playerBehavior)
    {
        bool isAtack = false;
        if (playerBehavior.isMove)
        {
            var a = playerController.GetNowPosGrid();
            var b = playerBehavior.move;
            (ActorController actor, Vec2D newVec, Pos2D _) = tilemapController.CoordinateMoveTo(a, b);
            if (actor == null && newVec.len == eLen.Zero) return; //もし壁にぶつかって結局動かないならターンを消費しない.
            if (actor != null) // もし敵にぶつかったならば攻撃とみなす.
            {
                playerBehavior.isMove = false;
                playerBehavior.move = newVec;
                isAtack = true;
            }
            // 両方でなければ通常移動.
        }
        if (playerBehavior.isMove)
        {
            status = Status.MOVE_E_DECIDE;
            var enemiesBehavior = DecideWhatToDo(playerBehavior);
            Dictionary<int, Behavior> moveEnemies = new Dictionary<int, Behavior>();
            Dictionary<int, Behavior> actEnemies = new Dictionary<int, Behavior>();
            for (int i = 0; i < enemiesBehavior.Length; i++) if (enemiesBehavior[i].isMove) moveEnemies.Add(i, enemiesBehavior[i]); else actEnemies.Add(i, enemiesBehavior[i]);

            status = Status.MOVE_PE_MOVE;
            await MoveActor(playerBehavior, moveEnemies); // awaitできるのはyield return null;だけがついたコルーチンだけ.

            status = Status.MOVE_E_ACT;
            await ActEnemy(actEnemies);
            // (同士討ちを実装するなら) 力尽きた敵を削除. (enemyControllersのリストが変更される)

            ChangeToKeyInput();
            status = Status.KEY_INPUT;
        }
        else
        {
            status = Status.ACT_P_ACT;
            if (isAtack) await MovePlayer(playerBehavior); else await ActPlayer(playerBehavior);
            // 力尽きた敵を削除. (enemyControllersのリストが変更される)

            status = Status.ACT_E_DECIDE;
            var enemiesBehavior = DecideWhatToDo(playerBehavior);
            Dictionary<int, Behavior> moveEnemies = new Dictionary<int, Behavior>();
            Dictionary<int, Behavior> actEnemies = new Dictionary<int, Behavior>();
            for (int i = 0; i < enemiesBehavior.Length; i++) if (enemiesBehavior[i].isMove) moveEnemies.Add(i, enemiesBehavior[i]); else actEnemies.Add(i, enemiesBehavior[i]);

            status = Status.ACT_E_ACT;
            await ActEnemy(actEnemies);

            status = Status.ACT_E_MOVE;
            await MoveEnemy(moveEnemies);

            ChangeToKeyInput();
            status = Status.KEY_INPUT;
        }
    }

    private Behavior[] DecideWhatToDo(Behavior playerBehavior)
    {
        int el = enemyControllers.Count;
        Behavior[] enemiesBehavior = new Behavior[el];

        var nowLoadingAnimation = StartCoroutine(nowLoadingController.NowLoadingAnimation());
        for (int i = 0; i < el; i++)
        {
            enemiesBehavior[i] = enemyControllers[i].decideBehavior(playerBehavior);
        }
        StopCoroutine(nowLoadingAnimation);
        nowLoadingController.DeleteText();

        return enemiesBehavior;
    }

    private async UniTask MoveActor(Behavior playerB, IReadOnlyDictionary<int, Behavior> enemiesB)
    {
        List<UniTask> arrayUniTask = new List<UniTask>();
        UniTask playerTask = playerController.Move(playerB.move, this.GetCancellationTokenOnDestroy());
        arrayUniTask.Add(playerTask);
        foreach(KeyValuePair<int, Behavior> eB in enemiesB)
        {
            UniTask enemyTask = enemyControllers[eB.Key].Move(eB.Value.move, this.GetCancellationTokenOnDestroy());
            arrayUniTask.Add(enemyTask);
        }
        await UniTask.WhenAll(arrayUniTask);
    }

    private async UniTask MovePlayer(Behavior playerB)
    {
        await playerController.Move(playerB.move, this.GetCancellationTokenOnDestroy(), true);
    }

    private async UniTask MoveEnemy(IReadOnlyDictionary<int, Behavior> enemiesB)
    {
        List<UniTask> arrayUniTask = new List<UniTask>();
        foreach (KeyValuePair<int, Behavior> eB in enemiesB)
        {
            UniTask enemyTask = enemyControllers[eB.Key].Move(eB.Value.move, this.GetCancellationTokenOnDestroy());
            arrayUniTask.Add(enemyTask);
        }
        await UniTask.WhenAll(arrayUniTask);
    }

    private async UniTask ActPlayer(Behavior playerB)
    {
        await playerController.Act(playerB.act, this.GetCancellationTokenOnDestroy());
    }

    private async UniTask ActEnemy(IReadOnlyDictionary<int, Behavior> enemiesB)
    {
        foreach (KeyValuePair<int, Behavior> eB in enemiesB)
        {
            await enemyControllers[eB.Key].Act(eB.Value.act, this.GetCancellationTokenOnDestroy());
        }
    }

    private void ChangeToKeyInput()
    {
        playerController.BeKeyInput();
        foreach(EnemyController e in enemyControllers) e.BeKeyInput();
        // 敵を発生する処理.
    }
}
