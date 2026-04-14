using NSMB.Utilities.Extensions;
using Quantum;
using UnityEngine;

namespace NSMB.Entities.World {
    public unsafe class DonutBlockAnimator : QuantumEntityViewComponent {

        //---Serialized Variables
        [SerializeField] private SpriteRenderer sRenderer;
        [SerializeField] private Sprite normalSprite, fallingSprite;

        [SerializeField] private float shakeSpeed = 4/60f, shakeAmount = 1/16f;

        public void OnValidate() {
            this.SetIfNull(ref sRenderer);
        }

        public override void OnUpdateView() {
            Frame f = PredictedFrame;
            if (!f.Unsafe.TryGetPointer(EntityRef, out DonutBlock* donutBlock)) {
                return;
            }

            if (!donutBlock->IsFalling && donutBlock->Timer <= (f.UpdateRate * 0.5f)) {
                transform.position += Vector3.right * (Mathf.Sin(2 * Mathf.PI * shakeSpeed * (donutBlock->Timer / 60f)) * shakeAmount);
            }
            sRenderer.sprite = donutBlock->IsFalling || donutBlock->Timer < donutBlock->FramesUntilFall ? fallingSprite : normalSprite;
        }
    }
}