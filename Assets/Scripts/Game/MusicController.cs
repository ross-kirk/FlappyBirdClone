using UnityEngine;

namespace Game
{
    public class MusicController : MonoBehaviour
    {
        [SerializeField] private MusicTrack _menuTrack;
        [SerializeField] private MusicTrack _gameTrack;
        

        public void QueueMenuMusic()
        {
            AudioComponent.Instance.Conductor.QueueAtNextBar(_menuTrack);
        }
        
        public void QueueGameMusic()
        {
            AudioComponent.Instance.Conductor.QueueAtNextBar(_gameTrack);
        }
    }
}