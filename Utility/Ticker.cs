using System;
using System.Timers;
using UnityEngine;

public class Ticker : MonoBehaviour
{
  public static float tickTime = 1 / 30f;

  private float _tickerTimer;

  public static event Action OnTickAction;

  private void Awake()
  {
    DontDestroyOnLoad(gameObject);
  }

  private void Update()
  {
    _tickerTimer += Time.deltaTime;
    if (_tickerTimer >= tickTime)
    {
      _tickerTimer = 0;
      OnTickAction?.Invoke();
    }
  }
}
