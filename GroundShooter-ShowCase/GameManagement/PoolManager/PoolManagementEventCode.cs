namespace LatteGames.PoolManagement
{
    [EventCode]
    public enum PoolManagementEventCode
    {
        OnCreatePoolItem,
        OnTakePoolItem,
        OnReturnPoolItem,
        OnDestroyPoolItem
    }
}