using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;

    Dictionary<string, PlayerController> players
        = new();

    public void MovePlayer(string nickname, Vector3 pos)
    {
        if(players.ContainsKey(nickname) == false)
            return;

        PlayerController player =
            players[nickname];

        if(player.isLocalPlayer)
            return;

        player.SetTargetPosition(pos);
    }

    public void CreatePlayer(string nickname, Vector3 pos)
    {
        Debug.Log($"CreatePlayer 호출 : {nickname}");
        
        // 이미 존재하면 생성 안 함
        if(players.ContainsKey(nickname))
            return;

        GameObject obj =
            Instantiate(playerPrefab);

        PlayerController player =
            obj.GetComponent<PlayerController>();

        player.nickname = nickname;

        player.isLocalPlayer =
        (
            nickname ==
            NetworkManager.Instance
                .socketClient
                .myNickname
        );

        // 최초 위치 설정
        player.transform.position = pos;

        // Remote 보간 초기값 동기화
        player.SetTargetPosition(pos);

        players[nickname] = player;

        Debug.Log($"Create Player : {nickname}");
    }
}