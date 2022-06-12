using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Movement
{
    public class UnitMovement : NetworkBehaviour
    {
        [SerializeField] private NavMeshAgent _agent = null;
        [SerializeField] private Targeter targeter = null;
        [SerializeField] private float chaseDistance = 8f;

        #region Server

        public override void OnStartServer()
        {
            GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
        }

        public override void OnStopServer()
        {
            GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
        }

        [ServerCallback]
        private void Update()
        {
            if (targeter.Target != null)
            {
                if ((targeter.Target.transform.position - transform.position).sqrMagnitude > (chaseDistance * chaseDistance))
                {
                    _agent.SetDestination(targeter.Target.transform.position); // chase
                }
                else if(_agent.hasPath) // stop chasing, but only do so if we have a path
                {
                    _agent.ResetPath();
                }
                
                return;
            }

            if (!_agent.hasPath) return; // avoid the risk of resetting the path the same frame as its calculated -> CmdValidateAndMovePlayer
            if (_agent.remainingDistance > _agent.stoppingDistance) return;
            _agent.ResetPath();
        }

        [Command]
        public void CmdValidateAndMovePlayer(Vector3 newPositon)
        {
            ServerMove(newPositon);
        }

        [Server]
        public void ServerMove(Vector3 position)
        {
            targeter.ClearTarget();
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
        }

        [Server]
        private void ServerHandleGameOver()
        {
            _agent.ResetPath();
        }

        #endregion

    }
}
