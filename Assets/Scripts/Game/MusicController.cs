using System;
using UnityEngine;

namespace Game
{
    public class MusicController : MonoBehaviour
    {
        [SerializeField] private MusicTrack _menuTrack;
        [SerializeField] private MusicTrack[] _gameTrack;
        
        [SerializeField] private GameStateController _gameStateController;

        private void OnEnable()
        {
            _gameStateController.OnStartGame += QueueGameMusic;
            _gameStateController.OnScoreUpdate += QueueGameMusicOnScore;
            _gameStateController.OnRestartGame += QueueMenuMusic;
        }

        private void OnDisable()
        {
            _gameStateController.OnStartGame -= QueueGameMusic;
            _gameStateController.OnScoreUpdate -= QueueGameMusicOnScore;
            _gameStateController.OnRestartGame -= QueueMenuMusic;
        }

        public void QueueMenuMusic()
        {
            AudioComponent.Instance.Conductor.QueueAtNextBar(_menuTrack);
        }
        
        public void QueueGameMusic()
        {
            AudioComponent.Instance.Conductor.QueueAtNextBar(_gameTrack[0]);
        }
        
        public void QueueGameMusicOnScore(int score)
        {
            if (score % 5 != 0)
                return;
            
            switch (score)
            {
                case 5:
                    AudioComponent.Instance.Conductor.QueueAtNextBar(_gameTrack[0]);
                    break;
                case 10:
                    AudioComponent.Instance.Conductor.QueueAtNextBar(_gameTrack[1]);
                    break;
                case 15:    
                    AudioComponent.Instance.Conductor.QueueAtNextBar(_gameTrack[2]);
                    break;
                case 20:
                    AudioComponent.Instance.Conductor.QueueAtNextBar(_gameTrack[3]);
                    break;
                case 25:
                    AudioComponent.Instance.Conductor.QueueAtNextBar(_gameTrack[4]);
                    break;
                case 30:
                    AudioComponent.Instance.Conductor.QueueAtNextBar(_gameTrack[5]);
                    break;
                default:
                    AudioComponent.Instance.Conductor.QueueAtNextBar(_gameTrack[0]);
                    break;
            }
        }

        public void StopMusic()
        {
            AudioComponent.Instance.Conductor.StopWithOutro();
        }
    }
}