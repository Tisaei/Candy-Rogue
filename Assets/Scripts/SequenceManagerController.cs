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
    public bool isResetEnemyControllerList { get; set; } = false;

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
        if (isResetEnemyControllerList) // Destroy()したフレームの最後で削除されるので次のフレームでリセット.(ButtonManagerControllerのUpdateより先に呼ばれる必要あり)
        {
            Debug.Log("Clear前\n" + string.Join(",\n", enemyControllers));
            enemyControllers.Clear();
            Debug.Log("Clear後\n" + string.Join(",\n", enemyControllers));
            enemyControllers.AddRange(Array.ConvertAll(GameObject.FindGameObjectsWithTag("Enemy"), g => g.GetComponent<EnemyController>()));
            Debug.Log("Add後\n" + string.Join(",\n", enemyControllers));
            isResetEnemyControllerList = false;
        }
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

    public List<EnemyController> GetEnemyControllerList() { return new List<EnemyController>(enemyControllers); }

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
        // 敵たちの行動を，矛盾がないように決める.
        int el = enemyControllers.Count;
        Behavior[] enemiesBehavior = new Behavior[el];

        var nowLoadingAnimation = StartCoroutine(nowLoadingController.NowLoadingAnimation());
        for (int i = 0; i < el; i++)
        {
            enemiesBehavior[i] = enemyControllers[i].decideBehavior(playerBehavior); // 一旦決めさせる.
        }

        List<Pos2D> KeepOutCoodinateList = new List<Pos2D>(); // 「行けない座標リスト(KOL)」を作る.
        List<int> enemyBehaviourUnConfirmedList = new List<int>(); // 「行動確定してない敵リスト(EBCL)」を作る.
        List<int> moveEIndex = new List<int>();
        for (int i = 0; i < el; i++)
        {
            if (!enemiesBehavior[i].isMove)
            {
                KeepOutCoodinateList.Add(enemyControllers[i].GetNowPosGrid()); // KOLにActの敵の座標を追加.
            } 
            else
            {
                moveEIndex.Add(i);
                enemyBehaviourUnConfirmedList.Add(i); // EBCLにMoveの敵のインテックスを追加.
            }
        }
        foreach(int i in moveEIndex)
        {
            if (!enemyBehaviourUnConfirmedList.Contains(i)) continue; // 自分の行動がすでに確定しているならcontinue.
            List<int> processingList = new List<int>();
            EnemyBehaviourToConsistency(i, ref processingList); // refがなくとも参照渡しになるが，分かりやすくするため明示的に指定.
        }
        bool EnemyBehaviourToConsistency(int n, ref List<int> processingList)
        {
            if (processingList.Contains(n)) return true; // nが処理中ならばtrueを返す.
            Vec2D moveVec = enemiesBehavior[n].move;
            Pos2D nowPos = enemyControllers[n].GetNowPosGrid();
            Pos2D goalPos = nowPos + moveVec.ToPos2D(); // 目標座標を計算.
            if (enemiesBehavior[n].move.len == eLen.Zero) // もし移動量が最初から0なら,
            {
                KeepOutCoodinateList.Add(goalPos); // KOLに追加.
                if (enemyBehaviourUnConfirmedList.Contains(n)) enemyBehaviourUnConfirmedList.Remove(n); // EBCLから削除.
                return false;
            }
            processingList.Add(n); // plにnを追加.
            do
            {
                Pos2D playerPos;
                if (playerBehavior.isMove) playerPos = playerController.GetNowPosGrid() + playerBehavior.move.ToPos2D(); else playerPos = playerController.GetNowPosGrid();
                if (KeepOutCoodinateList.Contains(goalPos) || playerPos.Equals(goalPos)) // 目標座標がKOLにあるor目標座標にプレイヤーがいる.
                {
                    moveVec.len--; // 移動ベクトルの移動量を1減らす.
                    goalPos = nowPos + moveVec.ToPos2D(); // 目標座標を再計算.
                }
                else
                {
                    int indexOfOtherEnemy = -1; // 目標座標に敵がいるとそのインデックスが入る.
                    foreach(int i in enemyBehaviourUnConfirmedList) { if (enemyControllers[i].GetNowPosGrid().Equals(goalPos)) { indexOfOtherEnemy = i; break; } } // 目標座標とほかの敵の座標を比較.
                    if(indexOfOtherEnemy != -1 && !EnemyBehaviourToConsistency(indexOfOtherEnemy, ref processingList)) // 目標座標に敵かいる and その敵が結局移動しない.
                    {
                        moveVec.len--; // 移動ベクトルの移動量を1減らす.
                        goalPos = nowPos + moveVec.ToPos2D(); // 目標座標を再計算.
                    }
                    else // 目標座標に敵がいない or いるけどその敵は結局動く.
                    {
                        if (processingList.Contains(n)) processingList.Remove(n); // plから削除.
                        enemiesBehavior[n].move = moveVec; // 目標座標を確定.
                        KeepOutCoodinateList.Add(goalPos); // KOLに追加.
                        if (enemyBehaviourUnConfirmedList.Contains(n)) enemyBehaviourUnConfirmedList.Remove(n); // EBCLから削除.
                        return true;
                    }
                }
            } while (moveVec.len == eLen.Zero);
            if (processingList.Contains(n)) processingList.Remove(n); // plから削除.
            enemiesBehavior[n].move = moveVec; // 目標座標を確定.
            KeepOutCoodinateList.Add(goalPos); // KOLに追加.
            if (enemyBehaviourUnConfirmedList.Contains(n)) enemyBehaviourUnConfirmedList.Remove(n); // EBCLから削除.
            return false;
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
        await playerController.Move(playerB.move, this.GetCancellationTokenOnDestroy(), true); // Player単体でMoveということは，攻撃を兼ねた移動だということ.
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
