using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<int> animationVar = new NetworkVariable<int>();
        public int currentAnimation;
        public int nextAnimation;
        public bool forward = true;
        public int TurnValue = 450;
        public int currentTUrn = 0;
        public float spawnTimer;
        public GameObject player;
        public Animator anim;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                Move();
            }

            anim = transform.gameObject.GetComponent<Animator>();
        }

        public void Move()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
                Position.Value = randomPosition;
            }
            else
            {
                SubmitPositionRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Position.Value = GetRandomPositionOnPlane();
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        void Update()
        {
            if (NetworkManager.Singleton.IsServer)
            {

                spawnTimer += Time.deltaTime;

                if (spawnTimer > 5)
                {
                    spawnTimer = 0;

                    int temp = Random.Range(1, 4);

                    animationVar.Value = temp;

                }
                //if (forward == true)
                //{
                //    Position.Value += new Vector3(0, 0.01f, 0);
                //    currentTUrn++;
                //}

                //if (forward == false)
                //{
                //    Position.Value -= new Vector3(0, 0.01f, 0);
                //    currentTUrn--;
                //}


                //if (currentTUrn > TurnValue)
                //{
                //    forward = false;
                //}

                //if (currentTUrn < 0)
                //{
                //    forward = true;
                //}


            }
            transform.position = Position.Value;
            nextAnimation = animationVar.Value;

            if (nextAnimation != currentAnimation)
            {
                ChangeAnimation(nextAnimation);
            }
        }

        //1 attack, 2 walk, 3 idle
        public void ChangeAnimation(int animationChangeTo)
        {
            if (animationChangeTo == 1)
            {
                anim.SetBool("Attack", true);
                anim.SetBool("Walking", false);

            }

            if (animationChangeTo == 2)
            {
                anim.SetBool("Attack", false);
                anim.SetBool("Walking", true);

            }

            if (animationChangeTo == 3)
            {
                anim.SetBool("Attack", false);
                anim.SetBool("Walking", false);
            }

        }
    }
}
