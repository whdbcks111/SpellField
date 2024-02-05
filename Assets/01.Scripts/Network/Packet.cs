using UnityEngine;

public class Packet
{
    public string SerializedMessage { get; private set; } = "";

    private string[] _splittedData = null;
    public int _readIndex = -1;

    public const char Separator = '\u001F';

    public Packet() { }

    public Packet(params object[] data) : base()
    {
        foreach(object o in data)
        {
            if (o is string str) WriteString(str);
            else if(o is int i) WriteInt(i);
            else if(o is float f) WriteFloat(f);
            else if(o is Vector2 v2) WriteVector2(v2);
            else if(o is Vector3 v3) WriteVector3(v3);
        }
    }

    public void ResetRead()
    {
        _readIndex = -1;
        _splittedData = null;
    }

    public Packet WriteString(string s)
    {
        SerializedMessage += s + Separator;
        return this;
    }

    public Packet WriteInt(int i)
    {
        WriteString(i.ToString());
        return this;
    }

    public Packet WriteFloat(float f, int offset = 3)
    {
        WriteString(string.Format("{0:F" + offset + "}", f));
        return this;
    }

    public Packet WriteVector2(Vector2 vec2)
    {
        WriteFloat(vec2.x);
        WriteFloat(vec2.y);
        return this;
    }

    public Packet WriteVector3(Vector3 vec2)
    {
        WriteFloat(vec2.x);
        WriteFloat(vec2.y);
        WriteFloat(vec2.z);
        return this;
    }

    public string NextString()
    {
        if(_readIndex++ == -1) _splittedData = SerializedMessage.Split(Separator);
        if (_splittedData.Length <= _readIndex) throw new System.Exception("End of Packet");
        return _splittedData[_readIndex];
    }

    public int NextInt()
    {
        string data = NextString();
        if (int.TryParse(data, out int i)) return i;
        throw new System.Exception("Parse failed for type: int");
    }

    public float NextFloat()
    {
        string data = NextString();
        if (float.TryParse(data, out float f)) return f;
        throw new System.Exception("Parse failed for type: float");
    }

    public Vector2 NextVector2()
    {
        try
        {
            float x = NextFloat();
            float y = NextFloat();
            return new Vector2(x, y);
        }
        catch (System.Exception)
        {
            throw new System.Exception("Parse failed for type: Vector2");
        }
    }

    public Vector3 NextVector3()
    {
        try
        {
            float x = NextFloat();
            float y = NextFloat();
            float z = NextFloat();
            return new Vector3(x, y, z);
        }
        catch (System.Exception)
        {
            throw new System.Exception("Parse failed for type: Vector3");
        }
    }
}