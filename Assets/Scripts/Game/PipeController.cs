using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class PipeController : MonoBehaviour
    {
        [SerializeField] private float pipeSpeed;
        [SerializeField] private float pipeSpawnDelay;
        [SerializeField] private GameObject pipePrefab;

        private bool shouldSpawn;
        private List<Pipe> pipes;
        private Vector3 screenBounds;

        private void Start()
        {
            screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
            InvokeRepeating("SpawnPipe", 1f, pipeSpawnDelay);
        }

        public void StartSpawner()
        {
            shouldSpawn = true;
        }

        public void StopSpawner()
        {
            shouldSpawn = false;
        }

        public void StartPipes()
        {
            foreach (var pipe in pipes)
            {
                pipe.SetSpeed(pipeSpeed);
            }
        }

        public void StopPipes()
        {
            foreach (var pipe in pipes)
            {
                pipe.SetSpeed(0);
            }
        }

        public void RemoveAllPipes()
        {
            foreach (var pipe in pipes)
            {
                pipe.DestroySelf();
            }
        }

        private void RemovePipe(Pipe pipeToRemove)
        {
            foreach (var pipe in pipes.ToList().Where(pipe => pipe == pipeToRemove))
            {
                pipes.Remove(pipe);
                pipeToRemove.OnPipeDestroyed -= RemovePipe;
            }
        }

        private void SpawnPipe()
        {
            if (!shouldSpawn)
            {
                return;
            }
            
            var yRandom = Random.Range(0, screenBounds.y);
            var pipe = Instantiate(pipePrefab, new Vector3(screenBounds.x + 30, yRandom, screenBounds.z),
                Quaternion.identity).GetComponent<Pipe>();
            pipe.OnPipeDestroyed += RemovePipe;
            pipes.Add(pipe);
        }
    }
}