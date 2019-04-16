﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;

public class GameManager : MonoBehaviour
{
    int gameRoomIndex;
    int gamePlayerIndex;
    ArrayList userlist;
    Vector3[] spawnPoints = new Vector3[6];
    public GameObject PlayerPrefab;
    public ArrayList players = new ArrayList();

    public Hashtable mapHashtable = new Hashtable();

    private SocketIOComponent socketIO;

    private void Awake()
    {
        //Spawning users
        UserData userData = GameObject.Find("UserData").GetComponent<UserData>();
        userlist = userData.getUserlist();
        gameRoomIndex = userData.getRoomIndex();
        gamePlayerIndex = userData.getPlayerIndex();

        GameObject networkmodule = GameObject.Find("NetworkModule");
        socketIO = networkmodule.GetComponent<NetworkModule>().get_socket();
    }

    void Start()
    {
        //Initialize Spawning Points
        spawnPoints[0] = new Vector3(1, 0, 0);
        spawnPoints[1] = new Vector3(-1, 0, 0);
        spawnPoints[2] = new Vector3(4, 0, 0);
        spawnPoints[3] = new Vector3(1, 0, 0);
        spawnPoints[4] = new Vector3(2, 0, 0);
        spawnPoints[5] = new Vector3(3, 0, 0);


        //Debug.Log("AAAAA"+userlist);
        //Debug.Log(userlist[0]);
        //Debug.Log(userlist[1]);
        int i = 0;
        foreach(User userData in userlist)
        {
            GameObject player = new GameObject();
            Debug.Log(userData.Num);
            player = Instantiate(PlayerPrefab, new Vector3(1, -2, 0), Quaternion.identity);
            player.GetComponent<PlayerDetail>().Controller = userData;
            player.AddComponent<BasePlayer>();
            players.Add(player);

            if (userData.Num == GameObject.Find("LocalPlayer").GetComponent<LocalPlayer>().getLocalPlayer().Num) //localPlayer의 index와 생성한 Player의 인덱스가 같으면
            {
                Debug.Log("Check local player");
                Debug.Log(player.GetComponent<PlayerDetail>().Controller.Num);
                player.AddComponent<Player>();
                GameObject.Find("Main Camera").GetComponent<followCamera>().setTarget(player.transform);
            }
        }

        socketIO.On("userGameMove", OnUserMove);
        socketIO.On("userGameAction", OnUserAction);
        socketIO.On("userBlockDestroy", OnBlockDestroy);

    }

    void Update()
    {
        
    }

    void OnUserMove(SocketIOEvent e)
    {
        //Debug.Log(e.name + " / " + e.data);
        JSONObject data = e.data;
        int userIndex = (int)data[0].n;
        JSONObject moveData = data[1];
        Vector3 newPosition = new Vector3(0, 0, 0);
        float toward;
        float velocityX, velocityY;
        newPosition.x = moveData[0].n;
        newPosition.y = moveData[1].n;
        newPosition.z = moveData[2].n;
        toward = moveData[3].n;
        velocityX = moveData[4].n;
        velocityY = moveData[5].n;
        
        //GameObject.Find
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerDetail>().Controller.Num == userIndex)
            {
                player.transform.SetPositionAndRotation(newPosition, Quaternion.identity);
                player.transform.localScale = new Vector3(toward, 1.5f);
                player.GetComponent<Rigidbody>().AddForce(new Vector3(velocityX, velocityY, 0));
                break;
                //Debug.Log("other player moved");
            }
        }
    }

    void OnUserAction(SocketIOEvent e)
    {
        JSONObject data = e.data;
        int userIndex = (int)data[0].n;
        JSONObject actionData = data[1];
        string actionTypeString = actionData[0].str;
        Player.ACTION_TYPE actionType = Player.ACTION_TYPE.TYPE_ATTACK;
        if (String.Equals(actionTypeString, Player.ACTION_TYPE.TYPE_ATTACK.ToString()))
        {
            actionType = Player.ACTION_TYPE.TYPE_ATTACK;
        }
        if (String.Equals(actionTypeString, Player.ACTION_TYPE.TYPE_SKILL.ToString()))
        {
            actionType = Player.ACTION_TYPE.TYPE_SKILL;
        }

        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerDetail>().Controller.Num == userIndex)
            {
                if (actionType == Player.ACTION_TYPE.TYPE_ATTACK)
                {
                    player.GetComponent<BasePlayer>().showAttackMotion();
                    break;
                }
                if (actionType == Player.ACTION_TYPE.TYPE_SKILL)
                {
                    player.GetComponent<BasePlayer>().showSkillMotion();
                    break;
                }
                //Debug.Log("called transform translate");
            }
        }
        Debug.Log(e.name + " / " + e.data);
    }

    void OnBlockDestroy(SocketIOEvent e)
    {
        JSONObject data = e.data;
        int destroyedBlock = (int)data[1].n;

        GameObject targetBlock = (GameObject)mapHashtable[destroyedBlock];
        Debug.Log(mapHashtable[destroyedBlock]);
        Debug.Log(destroyedBlock);
        targetBlock.GetComponent<DestroyBlock>().destroyBlock();
    }

    void OnUserHit(SocketIOEvent e)
    {
        Debug.Log(e.name + " / " + e.data);
    }

    public void sendUserMove(JSONObject data)
    {
        data.AddField("gameRoomIndex", gameRoomIndex);
        socketIO.Emit("userGameMove", data);
    }

    public void sendUserAction(JSONObject data)
    {
        data.AddField("gameRoomIndex", gameRoomIndex);
        socketIO.Emit("userGameAction", data);
    }

    public void sendBlockDestroy(JSONObject data)
    {
        data.AddField("gameRoomIndex", gameRoomIndex);
        socketIO.Emit("userBlockDestroy", data);
    }

    public void sendUserHit(JSONObject data)
    {
        data.AddField("gameRoomIndex", gameRoomIndex);
        socketIO.Emit("userGameHit", data);
    }

}
