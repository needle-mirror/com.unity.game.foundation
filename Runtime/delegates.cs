namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameItem"></param>
    public delegate void GameItemEventHandler(GameItem gameItem);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameItem"></param>
    /// <param name="stat"></param>
    /// <param name="value"></param>
    public delegate void StatChangedEventHandler
        (GameItem gameItem, StatDefinition stat, StatValue value);
}
