using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public class Packet
{
    //based on this packet class https://github.com/tom-weiland/tcp-udp-networking/blob/tutorial-part2/GameServer/GameServer/Packet.cs

    private List<byte> buffer;
    private byte[] readableBuffer;
    private char termination = '%';
    private bool disposed;
    //this should be overloaded by child classes
    protected int packetType = 0;

    public Packet()
    {
        buffer = new List<byte>();
    }

    public Packet(byte[] data)
    {
        buffer = new List<byte>();
        buffer.AddRange(data);
        readableBuffer = buffer.ToArray();
    }

    #region WriteData
    public void AddByte(byte value)
    {
        buffer.Add(value);
    }

    public void AddBytes(byte[] values)
    {
        buffer.AddRange(values);
    }

    public void AddInt(int value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void AddInts(int[] values)
    {
        for (int i = 0; i < values.Length; i++) AddInt(values[i]);
    }

    public void AddFloat(float value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void AddFloats(float[] values)
    {
        for (int i = 0; i < values.Length; i++) AddFloat(values[i]);
    }

    public void AddVec3(Vector3 value)
    {
        AddFloats(value.ToArray());
    }

    public void AddBool(bool value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }

    public void AddString(string value)
    {
        AddInt(value.Length);
        buffer.AddRange(Encoding.ASCII.GetBytes(value));
    }

    public void AddCommandString(string value)
    {
        buffer.AddRange(Encoding.ASCII.GetBytes(value));
    }
    #endregion

    #region ReadData

    public byte ReadByte(int offset)
    {
        if (readableBuffer.Length > offset) return readableBuffer[offset];
        else throw new Exception("Invalid index on byte read");
    }

    public byte[] ReadBytes(int offset, int count)
    {
        if (buffer.Count > offset + count) return buffer.GetRange(offset, count).ToArray();
        else throw new Exception("Invalid index or count on byte array read");
    }

    public int ReadInt(int offset)
    {
        if (readableBuffer.Length > offset + sizeof(int)) return BitConverter.ToInt32(readableBuffer, offset);
        else throw new Exception("Invald index on int read");
    }

    public float ReadFloat(int offset)
    {
        if (readableBuffer.Length > offset + sizeof(float)) return BitConverter.ToSingle(readableBuffer, offset);
        else throw new Exception("Invalid index on float read");
    }

    public Vector3 ReadVector3(int offset)
    {
        if (readableBuffer.Length > offset + 3 * sizeof(float))
        {
            float x = ReadFloat(offset);
            float y = ReadFloat(offset + sizeof(float));
            float z = ReadFloat(offset + 2 * sizeof(float));
            return new Vector3(x, y, z);
        }
        else throw new Exception("Invalid index on Vector3 read");
    }

    public bool ReadBool(int offset)
    {
        if (readableBuffer.Length > offset + sizeof(bool)) return BitConverter.ToBoolean(readableBuffer, offset);
        else throw new Exception("Invalid index on bool read");
    }

    public string ReadString(int offset)
    {
        int len = ReadInt(offset);
        if (readableBuffer.Length > offset + 1 + len) return Encoding.ASCII.GetString(readableBuffer, offset + 1, len);
        else throw new Exception("Invalid index on bool read");
    }

    #endregion

    public byte[] Pack()
    {
        AddCommandString(packetType.ToString());
        AddCommandString(termination.ToString());
        return buffer.ToArray();
    }

    public List<Packet> UnPack()
    {
        List<Packet> packets = new List<Packet>();
        int currentStart = 0;
        int currentPoint = 0;

        string fullBuffer = Encoding.ASCII.GetString(readableBuffer);

        for(; currentPoint < fullBuffer.Length; currentPoint++)
        {
            //if we've found the termination character
            if(fullBuffer[currentPoint] == termination && currentPoint != currentStart)
            {
                Packet newpacket = new Packet(buffer.GetRange(currentStart, currentPoint - currentStart).ToArray());
                packets.Add(newpacket);
                currentStart = currentPoint + 1;
            }
        }

        return packets;
    }

    public byte[] PackFront()
    {
        int lenght = buffer.Count;
        string AddFront = "$" + lenght.ToString() + "$";

        byte[] toAddFront = Encoding.ASCII.GetBytes(AddFront);

        //insert it at the beginning of the list
        buffer.InsertRange(0, toAddFront);
        return buffer.ToArray();
    }

    public List<Packet> UnPackFront()
    {
        List<Packet> packets = new List<Packet>();

        List<byte> ModifyBuffer = new List<byte>();
        ModifyBuffer.AddRange(readableBuffer);
        string currentBufferData = Encoding.ASCII.GetString(ModifyBuffer.ToArray());
        string[] splitData = currentBufferData.Split('$');

        do
        {
            //if we can find the lenght of the packet
            int len;
            if (int.TryParse(splitData[1], out len))
            {
                //then remove the lenght at the beginning
                string removeStr = "$" + splitData[1] + "$";
                int removeLen = Encoding.ASCII.GetBytes(removeStr).Length;
                ModifyBuffer.RemoveRange(0, removeLen);

                //if there's not enough data left, just exit
                if (len > ModifyBuffer.Count) break;

                //now do the copy
                Packet newPacket = new Packet(ModifyBuffer.GetRange(0, len).ToArray());
                packets.Add(newPacket);

                //and remove that data from the modify buffer
                ModifyBuffer.RemoveRange(0, len);

                //and get the data for the next iteration
                currentBufferData = Encoding.ASCII.GetString(ModifyBuffer.ToArray());
                splitData = currentBufferData.Split('$');
            }
            else break;
        }
        while (ModifyBuffer.Count > 0);

        return packets;
    }

    public int GetPacketType()
    {
        int inverseOffset = sizeof(char) + sizeof(int);
        int index = buffer.Count - inverseOffset;

        return ReadInt(index);
    }

    public byte[] RemoveExtraElements()
    {
        byte[] extra = Encoding.ASCII.GetBytes("0");
        int extraData = extra.Length;
        byte[] newData = new byte[buffer.Count - extraData];

        Array.Copy(buffer.ToArray(), 0, newData, 0, newData.Length);
        return newData;
    } 

    public byte[] GetDataRaw() => buffer.ToArray();

    public void Clear()
    {
        readableBuffer = null;
        buffer.Clear();
    }
}
