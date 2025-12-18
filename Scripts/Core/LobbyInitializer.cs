using UnityEngine;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyInitializer : MonoBehaviour
{
    void Start()
    {
        InitPhotonProps();
    }

    void InitPhotonProps()
    {
        if (PhotonNetwork.player != null)
        {
            Hashtable hash = new Hashtable();
            hash["SelectedTitan"] = null;
            hash["TeamIndex"] = null;
            hash["TeamSlot"] = null;
            PhotonNetwork.player.SetCustomProperties(hash);
        }

        Debug.Log("[LobbyInitializer] 로비 진입: SelectedTitan / TeamIndex / TeamSlot 초기화 완료");
    }
}
