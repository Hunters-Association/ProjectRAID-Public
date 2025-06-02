using System.Collections;

public interface IInitializable
{
    /// <summary>
    /// 매니저가 초기화 로직(설정 읽기, 서버 접속 등)을 비동기로 수행할 때 구현
    /// </summary>
    IEnumerator Initialize();
}