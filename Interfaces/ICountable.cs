using System;

public interface ICountable
{

    public event Action OnDeath;
    void OnSpawnAdToCounterListener();

}
