public interface IClient
{
    void OnEvent(string from, string eventName, string message);
    void OnJoinFailed(string reason);
    void OnJoinClient(string uid);
    void OnLeaveClient(string uid);
}
