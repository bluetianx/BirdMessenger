public class  MessageModel
{
    /// 发送者
    public string Sender{get;set;}
    ///接受者
    public string Receiver{get;set;}
    ///接受者客户端类型 
    public int ClientType{get;set;}
    ///消息类型
    public int MessageType {get;set;}
    ///消息内容
    public string Content{get;set;}
}