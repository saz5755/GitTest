using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string nickname;

    public bool isLocalPlayer;

    Vector3 targetPos;

    float sendTimer;

    void Update()
    {
        if (isLocalPlayer)
        {
            LocalUpdate();
        }
        else
        {
            RemoteUpdate();
        }
    }

    void LocalUpdate()
    {
        if (NetworkManager.Instance == null)
            return;

        if (NetworkManager.Instance.socketClient == null)
            return;

        if (NetworkManager.Instance.socketClient.IsConnected() == false)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        transform.position +=
            new Vector3(h, 0, v) * 5f * Time.deltaTime;

        sendTimer += Time.deltaTime;

        if (sendTimer >= 0.05f)
        {
            sendTimer = 0f;

            NetworkManager.Instance
                .socketClient
                .SendMove(transform.position);
        }
    }

    void RemoteUpdate()
    {
        transform.position =
            Vector3.Lerp(
                transform.position,
                targetPos,
                Time.deltaTime * 10f);
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPos = pos;
    }
}