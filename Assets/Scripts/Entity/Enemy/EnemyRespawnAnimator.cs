using NSMB.Particles;
using NSMB.Utilities.Extensions;
using Quantum;
using UnityEngine;
using static NSMB.Utilities.QuantumViewUtils;

namespace NSMB.Entities.Enemies {
    public unsafe class EnemyRespawnAnimator : QuantumEntityViewComponent {

        //---Serialized Variables
        [SerializeField] private AudioSource sfx;
        [SerializeField] private GameObject respawnParticle;
        [SerializeField] private Color respawnColor;

        //---Private Variables
        private GameObject activeRespawnParticle;

        public void OnValidate() {
            this.SetIfNull(ref sfx);
        }

        public void Start() {
            QuantumEvent.Subscribe<EventEnemyPreRespawned>(this, OnEnemyPreRespawned, FilterOutReplayFastForward);
            QuantumEvent.Subscribe<EventEnemyAfterDelayedRespawn>(this, OnEnemyAfterDelayedRespawn, FilterOutReplayFastForward);
        }

        private void OnEnemyPreRespawned(EventEnemyPreRespawned e) {
            if (e.Entity != EntityRef) {
                return;
            }

            if (activeRespawnParticle) {
                Destroy(activeRespawnParticle);
            }

            Frame f = PredictedFrame;
            var enemy = f.Unsafe.GetPointer<Enemy>(EntityRef);
            var collider2d = f.Unsafe.GetPointer<PhysicsCollider2D>(EntityRef);
            activeRespawnParticle = Instantiate(respawnParticle, enemy->Spawnpoint.ToUnityVector3() + collider2d->Shape.Centroid.ToUnityVector3(), Quaternion.identity);
            foreach (ParticleSystem particle in activeRespawnParticle.GetComponentsInChildren<ParticleSystem>()) {
                var main = particle.main;
                main.startColor = respawnColor;
            }

            sfx.PlayOneShot(SoundEffect.Player_Sound_Respawn, volume: 0.4f);
        }

        private void OnEnemyAfterDelayedRespawn(EventEnemyAfterDelayedRespawn e) {
            if (e.Entity != EntityRef) {
                return;
            }

            var enemy = PredictedFrame.Unsafe.GetPointer<Enemy>(e.Entity);
            MiscParticles.Instance.Play(ParticleEffect.Puff, enemy->Spawnpoint.ToUnityVector3() + (Vector3.up * 0.25f));
        }
    }
}