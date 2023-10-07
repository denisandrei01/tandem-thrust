using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : INetworkSerializable{

    public enum Role{
        None = 0,
        Pilot = 1,
        Copilot = 2
    }

    public ulong id;
    private FixedString128Bytes _name;
    public Role role;
    public int teamId;

    public string name
    {
        get => _name.Value;
    }

    public Player(){
        role = Role.None;
        teamId = -1;
    }

    public Player(ulong i, string p){
        if(p.Length >= 16){
            _name = p[..16];
        }
        _name = p;
        id = i;
        role = Role.None;
        teamId = -1;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref _name);
        serializer.SerializeValue(ref role);
        serializer.SerializeValue(ref teamId);
    }
    
}
