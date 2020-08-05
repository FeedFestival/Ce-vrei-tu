public enum Operation
{
    None,
    ClientHandshake,
    ServerHandshake,
    SimpleMessage,
    SendQuestion
}

public enum NetMessage
{
    None,
    Ok,
    No,
    StartingGame,
    DoPickCategory,
    ConnectionIsPickingCategory,
    ThisIsTheQuestion,
    LieAdded
}

[System.Serializable]
public abstract class NetMsg
{
    public byte OP { get; set; }

    public NetMsg()
    {
        OP = (byte)Operation.None;
    }
}

/////------------------/////------------------/////------------------/////------------------/////------------------/////------------------

[System.Serializable]
public class NetClientUser: NetMsg
{
    public NetClientUser()
    {
        OP = (byte)Operation.ClientHandshake;
    }

    public string Name { get; set; }
    public int Pic { get; set; }
    public int ConnId { get; set; }
}

[System.Serializable]
public class NetServerUsers : NetMsg
{
    public NetServerUsers()
    {
        OP = (byte)Operation.ServerHandshake;
    }

    public System.Collections.Generic.List<NetClientUser> Users { get; set; }
}

[System.Serializable]
public class SimpleMessage : NetMsg
{
    public SimpleMessage()
    {
        OP = (byte)Operation.SimpleMessage;
    }
    public byte MessageCode { get; set; }
    public byte ThisMessageCodeIsFor { get; set; }
    public int ConnId { get; set; }
    public string MessageText { get; set; }
    public int MessageId { get; set; }
}

[System.Serializable]
public class QuestionMessage : NetMsg
{
    public QuestionMessage()
    {
        OP = (byte)Operation.SendQuestion;
    }
    
    public Question Question { get; set; }
}