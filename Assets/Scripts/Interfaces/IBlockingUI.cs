public interface IBlockingUI
{
    /// 이 UI가 플레이어 상태를 막는 UI인지 여부
    bool BlocksGameplay { get; }
}