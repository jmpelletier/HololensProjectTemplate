using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/**
 *　このクラスはユーザーの視線を取得し、視線先の対象物について情報を管理している。
 */
public class GazeManager : MonoBehaviour {

    public static InstanceManager<GazeManager> instances = new InstanceManager<GazeManager>();

    // 視線の先に対象物がない場合、この距離（メートル）でデフォルトのターゲットを設定する。
    public float defaultGazeDistance = 3f;

    // 視線が新しくオブジェクトに当たった時に実行するアクション
    public UnityEvent GazeOnActions;

    // 視線がオブジェクトから離れたときに実行するアクション
    public UnityEvent GazeOffActions;

    public LayerMask raycastMask;

    // 視線先の点
    public Vector3 location
    {
        get;
        private set;
    }

    // 視線先の面のノーマル
    public Vector3 normal
    {
        get;
        private set;
    }

    // 視線先にColliderの付いているGameObjectがあるか
    public bool gazeOnObject
    {
        get;
        private set;
    }

    // 視線先のGameObject。Colliderがなければ、視線は通過する。
    public GameObject target
    {
        get;
        private set;
    }

    void Awake() {
        instances.Add(this);
	}

    //  視線が新しい対象物に当たった時の処理
    private void OnGazeOn ()
    {
        // 対象のGameObjectに付与されたコンポーネントに「GazeOn」メソッドがあれば実行する
        target.BroadcastMessage("GazeOn", SendMessageOptions.DontRequireReceiver);

        // Editorで登録されたアクションを実行する
        GazeOnActions.Invoke();
    }

    // 視線が対象物から離れた時の処理
    private void OnGazeOff (GameObject oldTarget)
    {
        // 対象のGameObjectに付与されたコンポーネントに「GazeOff」メソッドがあれば実行する
        oldTarget.BroadcastMessage("GazeOff", SendMessageOptions.DontRequireReceiver);

        // Editorで登録されたアクションを実行する
        GazeOffActions.Invoke();
    }

    // Canvasの中のUIに視線が当たっているかを確認する
    private bool GraphicRaycast()
    {
        List<RaycastResult> graphicRaycastResults = new List<RaycastResult>();

        // Canvas内のオブジェクトを見ているか
        if (EventSystem.current != null)
        {
            // 偽のマウス情報を作成する
            PointerEventData ped = new PointerEventData(EventSystem.current);

            // カーソルの位置は画面の中央（Hololensでは視線がいつも画面の中央だから）
            ped.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            // 当たっているオブジェクトを取得する
            EventSystem.current.RaycastAll(ped, graphicRaycastResults);

            // Linqを使って、対象物のリストをフィルタリングする
            RaycastResult raycastResult = graphicRaycastResults
                .Where(x => x.gameObject.GetComponent<Selectable>() != null) // Selectableを継承するコンポーネントを持つオブジェクトだけを残す
                .OrderBy(x => x.distance) // カメラから近い順で並び替える
                .ThenByDescending(x => x.depth) // そしてdepthが低い順で並び替える
                .FirstOrDefault(); // もっとも近いオブジェクトを選ぶ

            // 選択中のオブジェクトを強制的に指定する
            EventSystem.current.SetSelectedGameObject(raycastResult.gameObject, ped);

            // もし、UIオブジェクトに当たっていたら...
            if (raycastResult.gameObject != null)
            {
                // 情報を更新する
                location = raycastResult.worldPosition;
                normal = raycastResult.worldNormal;
                target = raycastResult.gameObject;
                return true;
            }
        }

        return false;
    }

    // Colliderを持つオブジェクトに視線が当たっているかを確認する
    private bool PhysicsRaycast()
    {
        var headPosition = Camera.main.transform.position; // 頭の位置
        var gazeDirection = Camera.main.transform.forward; // 視線の方向

        RaycastHit hitInfo;
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo, 1000f))
        {
            // Colliderに当たった、情報を更新する
            location = hitInfo.point;
            normal = hitInfo.normal;
            target = hitInfo.collider.gameObject;
            return true;
        } else
        {
            // そうでなければデフォルトの位置にする
            location = headPosition + gazeDirection * defaultGazeDistance;
            normal = gazeDirection * -1f;
            target = null;
        }

        return false;
    }

    void Update () {

        GameObject oldTarget = target;　// 古い対象物を覚えておく
        gazeOnObject = GraphicRaycast() || PhysicsRaycast(); // UIを先にチェックする

        if (oldTarget != null && target != oldTarget)
        {
            OnGazeOff(oldTarget);
        }
        if (target != null && target != oldTarget)
        {
            OnGazeOn();
        }
    }
}
