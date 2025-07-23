public enum WeaponType
{
    MainGun,
    ShoulderGadget,
    Drone,
    Armor,
    Ultimate,
}

public static class WeaponTypeExtensions
{
    public static CurrencyType ConvertToCurrencyType(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.MainGun => CurrencyType.MainGunToken,
            WeaponType.ShoulderGadget => CurrencyType.ShoulderGadgetToken,
            WeaponType.Drone => CurrencyType.DroneToken,
            WeaponType.Armor => CurrencyType.Standard,
            WeaponType.Ultimate => CurrencyType.UltimateToken,
            _ => CurrencyType.MainGunToken,
        };
    }
}