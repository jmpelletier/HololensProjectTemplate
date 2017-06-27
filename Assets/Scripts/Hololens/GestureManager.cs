using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.VR.WSA.Input;

public class GestureManager : MonoBehaviour {

    public static InstanceManager<GestureManager> instances = new InstanceManager<GestureManager>();

    // ジェスチャーを認識するオブジェクト
    GestureRecognizer recognizer;

    public UnityEvent tapActions;
    public UnityEvent pointerDownActions;
    public UnityEvent pointerUpActions;

    public delegate void OnTapDelegate();
    public event OnTapDelegate OnTap;

    public void PointerDown()
    {
        pointerDownActions.Invoke();
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ExecuteEvents.ExecuteHierarchy(EventSystem.current.currentSelectedGameObject, ped, ExecuteEvents.pointerDownHandler);
    }

    public void PointerUp()
    {
        pointerUpActions.Invoke();
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ExecuteEvents.ExecuteHierarchy(EventSystem.current.currentSelectedGameObject, ped, ExecuteEvents.pointerUpHandler);
    }

    public void Tap()
    {
        tapActions.Invoke();
        if (OnTap != null)
        {
            OnTap();
        }
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ExecuteEvents.ExecuteHierarchy(EventSystem.current.currentSelectedGameObject, ped, ExecuteEvents.pointerClickHandler);
    }

    void Awake() {
        instances.Add(this);

        InteractionManager.SourcePressed += (state) =>
        {
            PointerDown();
        };

        InteractionManager.SourceReleased += (state) =>
        {
            PointerUp();
        };

        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, count, ray) =>
        {
            Tap();
        };

        recognizer.StartCapturingGestures();
    }
}
