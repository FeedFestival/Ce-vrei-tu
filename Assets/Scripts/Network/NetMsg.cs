public enum Operation
{
    None,
    ClientHandshake,
    ServerHandshake
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

