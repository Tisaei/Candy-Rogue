using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using CandyRogueBase;

public class PlayerController : ActorController
{
    [SerializeField]
    private GameObject Camera, GameOverText;
    private CameraController cameraController;
    private GameOverTextController gameOverTextController;
    public override string ToString() { return base.ToString() + " Player"; }

    protected override void Start()
    {
        base.Start();
        cameraController = Camera.GetComponent<CameraController>();
        gameOverTextController = GameOverText.GetComponent<GameOverTextController>();
    }
    public override async UniTask Act(eAct act, CancellationToken cancellation_token)
    {
        cameraController.IsRotating = true;
        await base.Act(act, cancellation_token);
        cameraController.IsRotating = false;
    }

    public override void Attacked(int atk)
    {
        base.Attacked(atk);
        if(GetNowHp() == 0)
        {
            gameOverTextController.ShowGameOver();
        }
    }

    public override void SetStatus(Pos2D actorPos, eDir actorDir, int hp, int atk, int def)
    {
        base.SetStatus(actorPos, actorDir, hp, atk, def);
        gameObject.GetComponent<Renderer>().enabled = true;
        gameOverTextController.HideGameOver();
    }
}
