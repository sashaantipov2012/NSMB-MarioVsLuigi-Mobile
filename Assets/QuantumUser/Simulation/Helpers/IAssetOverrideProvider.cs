using System;

public interface IAssetOverrideProvider<E, T> where E : Enum {
    public T GetOverride(E value);
}