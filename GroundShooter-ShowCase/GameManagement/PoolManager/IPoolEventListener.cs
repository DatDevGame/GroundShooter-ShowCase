namespace LatteGames.PoolManagement
{
    public interface IPoolEventListener
    {
        void OnCreate();
        void OnTakeFromPool();
        void OnReturnToPool();
        void OnDispose();
    }
}