using UnityEngine;

[System.Serializable]
public class SerializableSprite : ISerializationCallbackReceiver
{
  [SerializeField]
  public string spritePath;

  private Sprite sprite;

  public Sprite Sprite => sprite;

  public SerializableSprite(string path)
  {
    spritePath = path;
  }

  public void OnAfterDeserialize()
  {
    throw new System.NotImplementedException();
  }

  public void OnBeforeSerialize()
  {
    throw new System.NotImplementedException();
  }
}