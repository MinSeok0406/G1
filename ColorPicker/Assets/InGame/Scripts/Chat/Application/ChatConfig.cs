using UnityEngine;

[CreateAssetMenu(fileName="ChatConfig", menuName="Game/Chat/Config")]
public class ChatConfig : ScriptableObject
{
    [Min(10)] public int MaxMessages = 150;
    [Min(1)]  public int MaxCharsPerMessage = 200;
    [Range(0.1f, 5f)] public float MinSendIntervalSec = 0.5f;
    [Min(10)] public int SyncBatchCount = 50;
    [Min(8)]  public int PoolDefaultCapacity = 32;
    [Min(16)] public int PoolMaxSize = 256;
}
