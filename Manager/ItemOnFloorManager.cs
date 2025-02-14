using System;
using UnityEngine;

public class ItemOnFloorManager : MonoBehaviour
{
    public ItemData item;

    [SerializeField]
    private AudioClip pickUpSound;
    [SerializeField]
    private AudioClip spawnSound;
    void Start()
    {
        if (item?.visual)
            gameObject.GetComponent<SpriteRenderer>().sprite = item.visual;
        SoundFXManager.Instance.PlaySoundFXClip(spawnSound, transform, 0.5f);
        // Animator animator = GetComponent<Animator>();
        // animator.SetTrigger("Idle");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log(Inventory.Instance);
            if (Inventory.Instance != null)
            {
                Action callback = () =>
                {
                    SoundFXManager.Instance.PlaySoundFXClip(pickUpSound, transform, 0.5f);
                    Destroy(gameObject);


                };

                if (item != null)
                {
                    Inventory.Instance.Add(item, 1, callback);
                }
            }
        }
    }

}
