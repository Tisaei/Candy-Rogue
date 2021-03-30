using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using CandyRogueBase;

public class PlayerController : ActorController
{
    [SerializeField]
    private GameObject Camera;
    private CameraController cameraController;

    protected override void Start()
    {
        base.Start();
        cameraController = Camera.GetComponent<CameraController>();
    }
    public override async UniTask Act(eAct act, CancellationToken cancellation_token)
    {
        cameraController.IsRotating = true;
        await base.Act(act, cancellation_token);
        cameraController.IsRotating = false;
    }
}
