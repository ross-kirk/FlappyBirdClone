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
        [SerializeField] private float spawnHeightLimit = 0.5f;
        
        private GameObject pipePrefab;
        private bool shouldSpawn;
        private bool pipesMoving;
        private List<Pipe> pipes = new List<Pipe>();
        private Vector3 screenBounds;

        private void Start()
        {
            screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
            InvokeRepeating("SpawnPipe", 1f, pipeSpawnDelay);
            pipePrefab = Resources.Load<GameObject>("Prefabs/Pipe");
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
            pipesMoving = true;
            foreach (var pipe in pipes)
            {
                pipe?.SetSpeed(pipeSpeed);
            }
        }

        public void StopPipes()
        {
            pipesMoving = false;
            foreach (var pipe in pipes)
            {
                pipe?.SetSpeed(0);
            }
        }

        public void RemoveAllPipes()
        {
            foreach (var pipe in pipes)
            {
                pipe?.DestroySelf();
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
            
            var yRandom = Random.Range(-screenBounds.y + spawnHeightLimit, screenBounds.y - spawnHeightLimit);
            var pipe = Instantiate(pipePrefab, new Vector3(screenBounds.x + 2, yRandom, screenBounds.z),
                Quaternion.identity).GetComponent<Pipe>();
            pipe.OnPipeDestroyed += RemovePipe;
            pipes.Add(pipe);
            if (pipesMoving)
            {
                pipe.SetSpeed(pipeSpeed);
            }
        }
    }
}